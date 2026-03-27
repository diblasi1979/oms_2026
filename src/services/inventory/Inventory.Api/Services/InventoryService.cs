using System.Data;
using Inventory.Api.Models;
using Microsoft.EntityFrameworkCore;
using Oms.Persistence;
using Oms.Persistence.Entities;

namespace Inventory.Api.Services;

public sealed class InventoryService
{
    private readonly OmsDbContext _dbContext;

    public InventoryService(OmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<InventoryItemResponse>> SearchAsync(string? sku, Guid? warehouseId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Inventory
            .AsNoTracking()
            .Include(item => item.Warehouse)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(sku))
        {
            query = query.Where(item => item.Sku.Contains(sku));
        }

        if (warehouseId.HasValue)
        {
            query = query.Where(item => item.WarehouseId == warehouseId.Value);
        }

        var items = await query
            .OrderBy(item => item.Warehouse.Name)
            .ThenBy(item => item.Sku)
            .ToListAsync(cancellationToken);

        return items.Select(Map).ToArray();
    }

    public async Task<IReadOnlyCollection<InventoryItemResponse>> ReserveAsync(ReserveStockRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var requestedSkus = request.Items.Select(item => item.Sku.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var items = await _dbContext.Inventory
            .Include(item => item.Warehouse)
            .Where(item => item.WarehouseId == request.WarehouseId && requestedSkus.Contains(item.Sku))
            .ToListAsync(cancellationToken);

        EnsureItemsExist(request, items);

        foreach (var line in request.Items)
        {
            var item = items.Single(current => current.Sku.Equals(line.Sku.Trim(), StringComparison.OrdinalIgnoreCase));
            _dbContext.Entry(item).Property(current => current.RowVersion).OriginalValue = DecodeConcurrencyToken(line.ConcurrencyToken, line.Sku);
            if (item.PhysicalStock - item.ReservedStock < line.Quantity)
            {
                throw new InvalidOperationException($"Stock insuficiente para {line.Sku}.");
            }
        }

        foreach (var line in request.Items)
        {
            var item = items.Single(current => current.Sku.Equals(line.Sku.Trim(), StringComparison.OrdinalIgnoreCase));
            item.ReservedStock += line.Quantity;
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("Conflicto de concurrencia al reservar stock. Reintenta la operación.");
        }

        return await SearchAsync(null, request.WarehouseId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<InventoryItemResponse>> ReleaseAsync(ReleaseStockRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(new ReserveStockRequest { WarehouseId = request.WarehouseId, Items = request.Items });

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var requestedSkus = request.Items.Select(item => item.Sku.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var items = await _dbContext.Inventory
            .Include(item => item.Warehouse)
            .Where(item => item.WarehouseId == request.WarehouseId && requestedSkus.Contains(item.Sku))
            .ToListAsync(cancellationToken);

        EnsureItemsExist(new ReserveStockRequest { WarehouseId = request.WarehouseId, Items = request.Items }, items);

        foreach (var line in request.Items)
        {
            var item = items.Single(current => current.Sku.Equals(line.Sku.Trim(), StringComparison.OrdinalIgnoreCase));
            _dbContext.Entry(item).Property(current => current.RowVersion).OriginalValue = DecodeConcurrencyToken(line.ConcurrencyToken, line.Sku);
            if (item.ReservedStock < line.Quantity)
            {
                throw new InvalidOperationException($"No se puede liberar más stock del reservado para {line.Sku}.");
            }
        }

        foreach (var line in request.Items)
        {
            var item = items.Single(current => current.Sku.Equals(line.Sku.Trim(), StringComparison.OrdinalIgnoreCase));
            item.ReservedStock -= line.Quantity;
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("Conflicto de concurrencia al liberar stock. Reintenta la operación.");
        }

        return await SearchAsync(null, request.WarehouseId, cancellationToken);
    }

    private static void ValidateRequest(ReserveStockRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.WarehouseId == Guid.Empty || request.Items.Count == 0)
        {
            throw new InvalidOperationException("La reserva requiere warehouse e items.");
        }

        if (request.Items.Any(item => string.IsNullOrWhiteSpace(item.Sku) || item.Quantity <= 0 || string.IsNullOrWhiteSpace(item.ConcurrencyToken)))
        {
            throw new InvalidOperationException("Cada item debe incluir SKU, cantidad mayor a cero y concurrencyToken.");
        }
    }

    private static void EnsureItemsExist(ReserveStockRequest request, IReadOnlyCollection<InventoryEntity> items)
    {
        var missingSkus = request.Items
            .Select(item => item.Sku.Trim())
            .Where(sku => items.All(item => !item.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (missingSkus.Length > 0)
        {
            throw new KeyNotFoundException($"No existe inventario para {string.Join(", ", missingSkus)} en el almacén {request.WarehouseId}.");
        }
    }

    private static InventoryItemResponse Map(InventoryEntity item) => new()
    {
        Sku = item.Sku,
        WarehouseId = item.WarehouseId,
        WarehouseName = item.Warehouse.Name,
        PhysicalStock = item.PhysicalStock,
        ReservedStock = item.ReservedStock,
        AvailableStock = item.AvailableStock,
        LocationCode = item.LocationCode,
        ConcurrencyToken = Convert.ToBase64String(item.RowVersion)
    };

    private static byte[] DecodeConcurrencyToken(string concurrencyToken, string sku)
    {
        try
        {
            return Convert.FromBase64String(concurrencyToken);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException($"ConcurrencyToken inválido para {sku}.", exception);
        }
    }
}
