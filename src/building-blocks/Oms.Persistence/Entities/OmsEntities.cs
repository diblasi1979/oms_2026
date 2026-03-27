namespace Oms.Persistence.Entities;

public sealed class WarehouseEntity
{
    public Guid WarehouseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<OrderEntity> Orders { get; set; } = new List<OrderEntity>();
    public ICollection<InventoryEntity> InventoryItems { get; set; } = new List<InventoryEntity>();
}

public sealed class OrderEntity
{
    public Guid OrderId { get; set; }
    public string Customer { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string DestinationCity { get; set; } = string.Empty;
    public string DestinationState { get; set; } = string.Empty;
    public string DestinationPostalCode { get; set; } = string.Empty;
    public decimal DestinationLatitude { get; set; }
    public decimal DestinationLongitude { get; set; }
    public Guid? AssignedWarehouseId { get; set; }
    public string? ShipmentTrackingNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public WarehouseEntity? AssignedWarehouse { get; set; }
    public ICollection<OrderItemEntity> Items { get; set; } = new List<OrderItemEntity>();
    public ICollection<OrderLogEntity> Logs { get; set; } = new List<OrderLogEntity>();
    public ICollection<ShipmentEntity> Shipments { get; set; } = new List<ShipmentEntity>();
}

public sealed class OrderItemEntity
{
    public Guid OrderItemId { get; set; }
    public Guid OrderId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public OrderEntity Order { get; set; } = null!;
}

public sealed class OrderLogEntity
{
    public Guid OrderLogId { get; set; }
    public Guid OrderId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime EventTimestamp { get; set; }
    public OrderEntity Order { get; set; } = null!;
}

public sealed class InventoryEntity
{
    public Guid InventoryId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public int PhysicalStock { get; set; }
    public int ReservedStock { get; set; }
    public int AvailableStock { get; private set; }
    public string LocationCode { get; set; } = string.Empty;
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public WarehouseEntity Warehouse { get; set; } = null!;
}

public sealed class ShipmentEntity
{
    public Guid ShipmentId { get; set; }
    public Guid OrderId { get; set; }
    public string Carrier { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal WeightKg { get; set; }
    public decimal HeightCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal LengthCm { get; set; }
    public decimal ShippingCost { get; set; }
    public string DestinationAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public OrderEntity Order { get; set; } = null!;
    public ICollection<ShipmentEventEntity> Events { get; set; } = new List<ShipmentEventEntity>();
}

public sealed class ShipmentEventEntity
{
    public Guid ShipmentEventId { get; set; }
    public Guid ShipmentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime EventTimestamp { get; set; }
    public ShipmentEntity Shipment { get; set; } = null!;
}

public sealed class ExternalWebhookEventEntity
{
    public Guid ExternalWebhookEventId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ExternalOrderId { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string CanonicalSnapshot { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
