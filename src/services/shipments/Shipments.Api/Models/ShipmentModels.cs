namespace Shipments.Api.Models;

public sealed class CreateShipmentRequest
{
    public Guid OrderId { get; set; }
    public string Customer { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public string DestinationAddress { get; set; } = string.Empty;
    public decimal WeightKg { get; set; }
    public decimal HeightCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal LengthCm { get; set; }
    public decimal ShippingCost { get; set; }
}

public sealed class ShipmentResponse
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public string Customer { get; init; } = string.Empty;
    public string Carrier { get; init; } = string.Empty;
    public string TrackingNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal WeightKg { get; init; }
    public decimal HeightCm { get; init; }
    public decimal WidthCm { get; init; }
    public decimal LengthCm { get; init; }
    public decimal ShippingCost { get; init; }
    public string DestinationAddress { get; init; } = string.Empty;
    public List<ShipmentEventResponse> Events { get; init; } = new();
}

public sealed class ShipmentEventResponse
{
    public DateTimeOffset Timestamp { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
}

public sealed class CarrierWebhookRequest
{
    public string Carrier { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public sealed class ShippingLabelResponse
{
    public Guid ShipmentId { get; init; }
    public string LabelFormat { get; init; } = "PLAINTEXT";
    public string Content { get; init; } = string.Empty;
}

internal sealed class ShipmentRecord
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public string Customer { get; init; } = string.Empty;
    public string Carrier { get; init; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal WeightKg { get; init; }
    public decimal HeightCm { get; init; }
    public decimal WidthCm { get; init; }
    public decimal LengthCm { get; init; }
    public decimal ShippingCost { get; init; }
    public string DestinationAddress { get; init; } = string.Empty;
    public List<ShipmentEventResponse> Events { get; init; } = new();
}
