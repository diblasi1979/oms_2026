namespace Orders.Api.Models;

public enum OrderStatus
{
    Pending,
    Preparing,
    Shipped,
    Delivered,
    Cancelled
}

public enum OrderOrigin
{
    Marketplace,
    Web,
    Manual
}

public sealed class CreateOrderRequest
{
    public string Customer { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public Guid CustomerTypeId { get; set; }
    public string DestinationCity { get; set; } = string.Empty;
    public string DestinationState { get; set; } = string.Empty;
    public string DestinationPostalCode { get; set; } = string.Empty;
    public decimal DestinationLatitude { get; set; }
    public decimal DestinationLongitude { get; set; }
    public OrderOrigin Origin { get; set; }
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

public sealed class CreateOrderItemRequest
{
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public sealed class OrderSummaryResponse
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string Customer { get; init; } = string.Empty;
    public Guid CustomerTypeId { get; init; }
    public string CustomerTypeCode { get; init; } = string.Empty;
    public string CustomerTypeName { get; init; } = string.Empty;
    public OrderStatus Status { get; init; }
    public OrderOrigin Origin { get; init; }
    public decimal Total { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public string AssignedWarehouse { get; init; } = string.Empty;
    public string DestinationCity { get; init; } = string.Empty;
}

public sealed class OrderDetailResponse
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string Customer { get; init; } = string.Empty;
    public Guid CustomerTypeId { get; init; }
    public string CustomerTypeCode { get; init; } = string.Empty;
    public string CustomerTypeName { get; init; } = string.Empty;
    public OrderStatus Status { get; init; }
    public OrderOrigin Origin { get; init; }
    public decimal Total { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public string AssignedWarehouse { get; init; } = string.Empty;
    public string DestinationCity { get; init; } = string.Empty;
    public string DestinationState { get; init; } = string.Empty;
    public string DestinationPostalCode { get; init; } = string.Empty;
    public string? ShipmentTrackingNumber { get; init; }
    public List<OrderItemResponse> Items { get; init; } = new();
    public List<OrderLogEntry> Logs { get; init; } = new();
}

public sealed class OrderItemResponse
{
    public string Sku { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Subtotal => Quantity * UnitPrice;
}

public sealed class OrderLogEntry
{
    public DateTimeOffset Timestamp { get; init; }
    public string Event { get; init; } = string.Empty;
    public string Details { get; init; } = string.Empty;
}

public sealed class WarehouseCandidate
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
}

public sealed class InventoryAllocation
{
    public Guid WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public double DistanceKm { get; init; }
}

public sealed class CustomerTypeResponse
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

public sealed class UpsertCustomerTypeRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed class CustomerResponse
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public Guid CustomerTypeId { get; init; }
    public string CustomerTypeCode { get; init; } = string.Empty;
    public string CustomerTypeName { get; init; } = string.Empty;
    public string AssignedPriceListName { get; init; } = string.Empty;
    public decimal InsuranceRatePercentage { get; init; }
    public bool IsActive { get; init; }
}

public sealed class UpsertCustomerRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid CustomerTypeId { get; set; }
    public string AssignedPriceListName { get; set; } = string.Empty;
    public decimal InsuranceRatePercentage { get; set; }
    public bool IsActive { get; set; } = true;
}

internal sealed class OrderRecord
{
    public Guid Id { get; init; }
    public string Customer { get; init; } = string.Empty;
    public OrderStatus Status { get; set; }
    public OrderOrigin Origin { get; init; }
    public decimal Total { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public string DestinationCity { get; init; } = string.Empty;
    public string DestinationState { get; init; } = string.Empty;
    public string DestinationPostalCode { get; init; } = string.Empty;
    public decimal DestinationLatitude { get; init; }
    public decimal DestinationLongitude { get; init; }
    public WarehouseCandidate AssignedWarehouse { get; init; } = new();
    public string? ShipmentTrackingNumber { get; set; }
    public List<OrderItemResponse> Items { get; init; } = new();
    public List<OrderLogEntry> Logs { get; init; } = new();
}
