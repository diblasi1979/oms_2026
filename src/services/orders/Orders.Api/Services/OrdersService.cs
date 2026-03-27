using System.Data;
using Microsoft.EntityFrameworkCore;
using Oms.Persistence;
using Oms.Persistence.Entities;
using Orders.Api.Models;

namespace Orders.Api.Services;

public sealed class OrdersService
{
    private readonly OmsDbContext _dbContext;

    public OrdersService(OmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<OrderSummaryResponse>> GetOrdersAsync(OrderStatus? status, string? search, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.AssignedWarehouse)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(order => order.Status == status.Value.ToString());
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            var parsedOrderId = Guid.TryParse(normalizedSearch, out var orderId);

            query = query.Where(order =>
                order.Customer.Contains(normalizedSearch) ||
                order.DestinationCity.Contains(normalizedSearch) ||
                (parsedOrderId && order.OrderId == orderId) ||
                order.Items.Any(item => item.Sku.Contains(normalizedSearch)));
        }

        var orders = await query
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(MapSummary).ToArray();
    }

    public async Task<OrderDetailResponse?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .AsNoTracking()
            .Include(current => current.AssignedWarehouse)
            .Include(current => current.Items)
            .Include(current => current.Logs)
            .SingleOrDefaultAsync(current => current.OrderId == orderId, cancellationToken);

        return order is null ? null : MapDetail(order);
    }

    public async Task<OrderDetailResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Customer))
        {
            throw new InvalidOperationException("El cliente es obligatorio.");
        }

        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("La orden debe contener al menos un item.");
        }

        if (request.Items.Any(item => item.Quantity <= 0 || item.UnitPrice <= 0 || string.IsNullOrWhiteSpace(item.Sku)))
        {
            throw new InvalidOperationException("Cada item debe tener SKU, cantidad y precio unitario válidos.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var allocation = await SelectWarehouseAsync(request, cancellationToken);
        var requestItemsBySku = request.Items.ToDictionary(item => item.Sku.Trim(), StringComparer.OrdinalIgnoreCase);
        var inventoryRows = await _dbContext.Inventory
            .Where(item => item.WarehouseId == allocation.WarehouseId && requestItemsBySku.Keys.Contains(item.Sku))
            .ToListAsync(cancellationToken);

        foreach (var inventoryItem in inventoryRows)
        {
            var requestedItem = requestItemsBySku[inventoryItem.Sku];
            if (inventoryItem.PhysicalStock - inventoryItem.ReservedStock < requestedItem.Quantity)
            {
                throw new InvalidOperationException($"Stock insuficiente para {inventoryItem.Sku}.");
            }
        }

        foreach (var inventoryItem in inventoryRows)
        {
            inventoryItem.ReservedStock += requestItemsBySku[inventoryItem.Sku].Quantity;
        }

        var timestamp = DateTime.UtcNow;
        var orderEntity = new OrderEntity
        {
            OrderId = Guid.NewGuid(),
            Customer = request.Customer.Trim(),
            Status = OrderStatus.Pending.ToString(),
            Origin = request.Origin.ToString(),
            Total = request.Items.Sum(item => item.Quantity * item.UnitPrice),
            DestinationCity = request.DestinationCity,
            DestinationState = request.DestinationState,
            DestinationPostalCode = request.DestinationPostalCode,
            DestinationLatitude = request.DestinationLatitude,
            DestinationLongitude = request.DestinationLongitude,
            AssignedWarehouseId = allocation.WarehouseId,
            CreatedAt = timestamp,
            UpdatedAt = timestamp,
            Items = request.Items.Select(item => new OrderItemEntity
            {
                OrderItemId = Guid.NewGuid(),
                Sku = item.Sku.Trim(),
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList(),
            Logs =
            {
                new OrderLogEntity { OrderLogId = Guid.NewGuid(), EventName = "OrderCreated", Details = $"Orden creada desde {request.Origin}.", EventTimestamp = timestamp },
                new OrderLogEntity { OrderLogId = Guid.NewGuid(), EventName = "StockReserved", Details = "Stock reservado para evitar overselling.", EventTimestamp = timestamp },
                new OrderLogEntity { OrderLogId = Guid.NewGuid(), EventName = "WarehouseAssigned", Details = $"Asignado a {allocation.WarehouseName} a {allocation.DistanceKm:F1} km del destino.", EventTimestamp = timestamp }
            }
        };

        await _dbContext.Orders.AddAsync(orderEntity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await GetByIdAsync(orderEntity.OrderId, cancellationToken)
            ?? throw new InvalidOperationException("La orden se creó pero no pudo recuperarse.");
    }

    public async Task<OrderDetailResponse> UpdateStatusAsync(Guid orderId, OrderStatus status, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .Include(current => current.AssignedWarehouse)
            .Include(current => current.Items)
            .Include(current => current.Logs)
            .SingleOrDefaultAsync(current => current.OrderId == orderId, cancellationToken)
            ?? throw new KeyNotFoundException("La orden no existe.");

        order.Status = status.ToString();
        order.UpdatedAt = DateTime.UtcNow;
        order.Logs.Add(new OrderLogEntity
        {
            OrderLogId = Guid.NewGuid(),
            EventName = "OrderStatusChanged",
            Details = $"Estado actualizado a {status}.",
            EventTimestamp = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapDetail(order);
    }

    private async Task<InventoryAllocation> SelectWarehouseAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var requestedSkus = request.Items.Select(item => item.Sku.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var warehouses = await _dbContext.Warehouses
            .AsNoTracking()
            .Select(warehouse => new WarehouseCandidate
            {
                Id = warehouse.WarehouseId,
                Name = warehouse.Name,
                Latitude = warehouse.Latitude,
                Longitude = warehouse.Longitude
            })
            .ToListAsync(cancellationToken);

        var inventoryRows = await _dbContext.Inventory
            .AsNoTracking()
            .Where(item => requestedSkus.Contains(item.Sku))
            .ToListAsync(cancellationToken);

        var candidates = warehouses
            .Where(warehouse => CanFulfill(warehouse.Id, request.Items, inventoryRows))
            .Select(warehouse => new InventoryAllocation
            {
                WarehouseId = warehouse.Id,
                WarehouseName = warehouse.Name,
                DistanceKm = CalculateDistanceKm(warehouse.Latitude, warehouse.Longitude, request.DestinationLatitude, request.DestinationLongitude)
            })
            .OrderBy(candidate => candidate.DistanceKm)
            .ToList();

        if (candidates.Count == 0)
        {
            throw new InvalidOperationException("No existe un almacén con stock disponible para cubrir la orden completa.");
        }

        return candidates[0];
    }

    private static bool CanFulfill(Guid warehouseId, IEnumerable<CreateOrderItemRequest> items, IEnumerable<InventoryEntity> inventoryRows)
    {
        var stockBySku = inventoryRows.Where(item => item.WarehouseId == warehouseId)
            .ToDictionary(item => item.Sku, StringComparer.OrdinalIgnoreCase);

        return items.All(item =>
            stockBySku.TryGetValue(item.Sku.Trim(), out var stock) &&
            stock.PhysicalStock - stock.ReservedStock >= item.Quantity);
    }

    private static OrderSummaryResponse MapSummary(OrderEntity order) => new()
    {
        Id = order.OrderId,
        Customer = order.Customer,
        Status = Enum.Parse<OrderStatus>(order.Status, true),
        Origin = Enum.Parse<OrderOrigin>(order.Origin, true),
        Total = order.Total,
        Timestamp = new DateTimeOffset(DateTime.SpecifyKind(order.CreatedAt, DateTimeKind.Utc)),
        AssignedWarehouse = order.AssignedWarehouse?.Name ?? string.Empty,
        DestinationCity = order.DestinationCity
    };

    private static OrderDetailResponse MapDetail(OrderEntity order) => new()
    {
        Id = order.OrderId,
        Customer = order.Customer,
        Status = Enum.Parse<OrderStatus>(order.Status, true),
        Origin = Enum.Parse<OrderOrigin>(order.Origin, true),
        Total = order.Total,
        Timestamp = new DateTimeOffset(DateTime.SpecifyKind(order.CreatedAt, DateTimeKind.Utc)),
        AssignedWarehouse = order.AssignedWarehouse?.Name ?? string.Empty,
        DestinationCity = order.DestinationCity,
        DestinationState = order.DestinationState,
        DestinationPostalCode = order.DestinationPostalCode,
        ShipmentTrackingNumber = order.ShipmentTrackingNumber,
        Items = order.Items.Select(item => new OrderItemResponse
        {
            Sku = item.Sku,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice
        }).ToList(),
        Logs = order.Logs
            .OrderByDescending(log => log.EventTimestamp)
            .Select(log => new OrderLogEntry
            {
                Timestamp = new DateTimeOffset(DateTime.SpecifyKind(log.EventTimestamp, DateTimeKind.Utc)),
                Event = log.EventName,
                Details = log.Details
            }).ToList()
    };

    private static double CalculateDistanceKm(decimal originLatitude, decimal originLongitude, decimal destinationLatitude, decimal destinationLongitude)
    {
        const double EarthRadiusKm = 6371d;
        var lat1 = DegreesToRadians((double)originLatitude);
        var lon1 = DegreesToRadians((double)originLongitude);
        var lat2 = DegreesToRadians((double)destinationLatitude);
        var lon2 = DegreesToRadians((double)destinationLongitude);

        var dLat = lat2 - lat1;
        var dLon = lon2 - lon1;
        var a = Math.Pow(Math.Sin(dLat / 2), 2) +
                Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dLon / 2), 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180d;

}
