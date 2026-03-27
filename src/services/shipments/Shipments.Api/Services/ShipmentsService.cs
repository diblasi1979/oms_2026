using Microsoft.EntityFrameworkCore;
using Oms.Persistence;
using Oms.Persistence.Entities;
using Shipments.Api.Models;

namespace Shipments.Api.Services;

public sealed class ShipmentsService
{
    private readonly OmsDbContext _dbContext;
    private readonly ShipmentPricingService _shipmentPricingService;

    public ShipmentsService(OmsDbContext dbContext, ShipmentPricingService shipmentPricingService)
    {
        _dbContext = dbContext;
        _shipmentPricingService = shipmentPricingService;
    }

    public async Task<IReadOnlyCollection<ShipmentResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var shipments = await _dbContext.Shipments
            .AsNoTracking()
            .Include(shipment => shipment.Order)
            .Include(shipment => shipment.Events)
            .OrderByDescending(shipment => shipment.CreatedAt)
            .ToListAsync(cancellationToken);

        return shipments.Select(Map).ToArray();
    }

    public async Task<ShipmentResponse?> GetByIdAsync(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        var shipment = await _dbContext.Shipments
            .AsNoTracking()
            .Include(current => current.Order)
            .Include(current => current.Events)
            .SingleOrDefaultAsync(current => current.ShipmentId == shipmentId, cancellationToken);

        return shipment is null ? null : Map(shipment);
    }

    public async Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request, CancellationToken cancellationToken = default)
    {
        if (request.OrderId == Guid.Empty || request.CarrierId == Guid.Empty || string.IsNullOrWhiteSpace(request.Customer))
        {
            throw new InvalidOperationException("El envío requiere orderId, cliente y un carrier válido.");
        }

        var order = await _dbContext.Orders
            .Include(current => current.Shipments)
            .SingleOrDefaultAsync(current => current.OrderId == request.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException("La orden asociada al envío no existe.");

        var carrier = await _dbContext.Carriers
            .SingleOrDefaultAsync(current => current.CarrierId == request.CarrierId, cancellationToken)
            ?? throw new KeyNotFoundException("El carrier seleccionado no existe.");

        if (!carrier.IsActive)
        {
            throw new InvalidOperationException("El carrier seleccionado está inactivo.");
        }

        if (request.IncludeInsurance && !carrier.InsuranceSupported)
        {
            throw new InvalidOperationException("El carrier seleccionado no admite seguro.");
        }

        var pricingQuote = await _shipmentPricingService.QuoteAsync(order.DestinationPostalCode, request.IncludeInsurance, cancellationToken);

        var shipment = new ShipmentEntity
        {
            ShipmentId = Guid.NewGuid(),
            OrderId = request.OrderId,
            CarrierId = carrier.CarrierId,
            Carrier = carrier.Name,
            TrackingNumber = $"TRK-{DateTimeOffset.UtcNow:yyyyMMdd}-{Random.Shared.Next(10000, 99999)}",
            Status = "LabelCreated",
            WeightKg = request.WeightKg,
            HeightCm = request.HeightCm,
            WidthCm = request.WidthCm,
            LengthCm = request.LengthCm,
            ShippingCost = pricingQuote.TotalShippingCost,
            DestinationAddress = request.DestinationAddress,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Events =
            {
                new ShipmentEventEntity
                {
                    ShipmentEventId = Guid.NewGuid(),
                    Status = "LabelCreated",
                    Notes = $"Etiqueta simulada generada en OMS. Tarifa base {pricingQuote.BaseShippingCost:0.00} + seguro {pricingQuote.InsuranceCost:0.00}.",
                    EventTimestamp = DateTime.UtcNow
                }
            }
        };

        order.ShipmentTrackingNumber = shipment.TrackingNumber;
        order.UpdatedAt = DateTime.UtcNow;

        await _dbContext.Shipments.AddAsync(shipment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(shipment.ShipmentId, cancellationToken)
            ?? throw new InvalidOperationException("El envío se creó pero no pudo recuperarse.");
    }

    public async Task<ShipmentResponse> UpdateStatusAsync(CarrierWebhookRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.TrackingNumber) || string.IsNullOrWhiteSpace(request.Status))
        {
            throw new InvalidOperationException("El webhook requiere tracking number y estado.");
        }

        var shipment = await _dbContext.Shipments
            .Include(current => current.Order)
            .Include(current => current.Events)
            .SingleOrDefaultAsync(current => current.TrackingNumber == request.TrackingNumber, cancellationToken)
            ?? throw new KeyNotFoundException("No se encontró un envío con el tracking informado.");

        shipment.Status = request.Status;
        shipment.UpdatedAt = DateTime.UtcNow;
        shipment.Events.Add(new ShipmentEventEntity
        {
            ShipmentEventId = Guid.NewGuid(),
            Status = request.Status,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? "Actualización recibida desde el carrier." : request.Notes,
            EventTimestamp = DateTime.UtcNow
        });

        shipment.Order.ShipmentTrackingNumber = shipment.TrackingNumber;
        shipment.Order.UpdatedAt = DateTime.UtcNow;

        var mappedOrderStatus = request.Status.ToLowerInvariant() switch
        {
            "intransit" => "Shipped",
            "delivered" => "Delivered",
            "cancelled" => "Cancelled",
            _ => shipment.Order.Status
        };

        shipment.Order.Status = mappedOrderStatus;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(shipment);
    }

    public async Task<ShippingLabelResponse> GenerateLabelAsync(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        var shipment = await _dbContext.Shipments
            .AsNoTracking()
            .SingleOrDefaultAsync(current => current.ShipmentId == shipmentId, cancellationToken)
            ?? throw new KeyNotFoundException("El envío no existe.");

        return new ShippingLabelResponse
        {
            ShipmentId = shipment.ShipmentId,
            Content = string.Join(Environment.NewLine,
            [
                "OMS SHIPPING LABEL",
                $"Shipment: {shipment.ShipmentId}",
                $"Order: {shipment.OrderId}",
                $"Carrier: {shipment.Carrier}",
                $"Tracking: {shipment.TrackingNumber}",
                $"Destination: {shipment.DestinationAddress}",
                $"WeightKg: {shipment.WeightKg:F2}"
            ])
        };
    }

    public async Task<ShippingLabelResponse> GenerateLabelByOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var shipmentId = await _dbContext.Shipments
            .AsNoTracking()
            .Where(current => current.OrderId == orderId)
            .Select(current => current.ShipmentId)
            .SingleOrDefaultAsync(cancellationToken);

        if (shipmentId == Guid.Empty)
        {
            throw new KeyNotFoundException("La orden no tiene un envío asociado.");
        }

        return await GenerateLabelAsync(shipmentId, cancellationToken);
    }

    private static ShipmentResponse Map(ShipmentEntity shipment) => new()
    {
        Id = shipment.ShipmentId,
        OrderId = shipment.OrderId,
        CarrierId = shipment.CarrierId,
        Customer = shipment.Order.Customer,
        Carrier = shipment.Carrier,
        TrackingNumber = shipment.TrackingNumber,
        Status = shipment.Status,
        WeightKg = shipment.WeightKg,
        HeightCm = shipment.HeightCm,
        WidthCm = shipment.WidthCm,
        LengthCm = shipment.LengthCm,
        BaseShippingCost = shipment.ShippingCost,
        InsuranceCost = 0m,
        ShippingCost = shipment.ShippingCost,
        DestinationPostalCode = shipment.Order.DestinationPostalCode,
        DestinationAddress = shipment.DestinationAddress,
        Events = shipment.Events.OrderByDescending(@event => @event.EventTimestamp).Select(@event => new ShipmentEventResponse
        {
            Timestamp = new DateTimeOffset(DateTime.SpecifyKind(@event.EventTimestamp, DateTimeKind.Utc)),
            Status = @event.Status,
            Notes = @event.Notes
        }).ToList()
    };
}
