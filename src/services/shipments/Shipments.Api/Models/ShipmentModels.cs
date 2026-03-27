namespace Shipments.Api.Models;

public sealed class CreateShipmentRequest
{
    public Guid OrderId { get; set; }
    public string Customer { get; set; } = string.Empty;
    public Guid CarrierId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public string DestinationAddress { get; set; } = string.Empty;
    public decimal WeightKg { get; set; }
    public decimal HeightCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal LengthCm { get; set; }
    public bool IncludeInsurance { get; set; } = true;
}

public sealed class ShipmentResponse
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public Guid? CarrierId { get; init; }
    public string Customer { get; init; } = string.Empty;
    public string RecipientName { get; init; } = string.Empty;
    public string RecipientPhone { get; init; } = string.Empty;
    public string RecipientEmail { get; init; } = string.Empty;
    public string Carrier { get; init; } = string.Empty;
    public string TrackingNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal WeightKg { get; init; }
    public decimal HeightCm { get; init; }
    public decimal WidthCm { get; init; }
    public decimal LengthCm { get; init; }
    public decimal BaseShippingCost { get; init; }
    public decimal InsuranceCost { get; init; }
    public decimal ShippingCost { get; init; }
    public string DestinationPostalCode { get; init; } = string.Empty;
    public string DestinationAddress { get; init; } = string.Empty;
    public List<ShipmentEventResponse> Events { get; init; } = new();
}

public sealed class CarrierResponse
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string ServiceLevel { get; init; } = string.Empty;
    public string TrackingUrlTemplate { get; init; } = string.Empty;
    public string SupportEmail { get; init; } = string.Empty;
    public string SupportPhone { get; init; } = string.Empty;
    public bool InsuranceSupported { get; init; }
    public bool IsActive { get; init; }
    public string Notes { get; init; } = string.Empty;
}

public sealed class UpsertCarrierRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ServiceLevel { get; set; } = string.Empty;
    public string TrackingUrlTemplate { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
    public string SupportPhone { get; set; } = string.Empty;
    public bool InsuranceSupported { get; set; }
    public bool IsActive { get; set; } = true;
    public string Notes { get; set; } = string.Empty;
}

public sealed class ShipmentPricingSettingsResponse
{
    public decimal DefaultBaseCost { get; init; }
    public decimal InsuranceFlatCost { get; init; }
    public List<ShipmentPricingRuleResponse> Rules { get; init; } = new();
}

public sealed class ShipmentPricingRuleResponse
{
    public Guid Id { get; init; }
    public string RuleName { get; init; } = string.Empty;
    public string PostalCodePrefix { get; init; } = string.Empty;
    public decimal BaseCost { get; init; }
}

public sealed class UpdateShipmentPricingSettingsRequest
{
    public decimal DefaultBaseCost { get; set; }
    public decimal InsuranceFlatCost { get; set; }
    public List<UpdateShipmentPricingRuleRequest> Rules { get; set; } = new();
}

public sealed class UpdateShipmentPricingRuleRequest
{
    public Guid? Id { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string PostalCodePrefix { get; set; } = string.Empty;
    public decimal BaseCost { get; set; }
}

public sealed class ShipmentPricingQuoteRequest
{
    public string DestinationPostalCode { get; set; } = string.Empty;
    public bool IncludeInsurance { get; set; } = true;
}

public sealed class ShipmentPricingQuoteResponse
{
    public string DestinationPostalCode { get; init; } = string.Empty;
    public string? MatchedRuleName { get; init; }
    public string? MatchedPostalCodePrefix { get; init; }
    public bool UsedDefaultRate { get; init; }
    public decimal BaseShippingCost { get; init; }
    public decimal InsuranceCost { get; init; }
    public decimal TotalShippingCost { get; init; }
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
    public string RecipientName { get; init; } = string.Empty;
    public string RecipientPhone { get; init; } = string.Empty;
    public string RecipientEmail { get; init; } = string.Empty;
    public string Carrier { get; init; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal WeightKg { get; init; }
    public decimal HeightCm { get; init; }
    public decimal WidthCm { get; init; }
    public decimal LengthCm { get; init; }
    public decimal BaseShippingCost { get; init; }
    public decimal InsuranceCost { get; init; }
    public decimal ShippingCost { get; init; }
    public string DestinationPostalCode { get; init; } = string.Empty;
    public string DestinationAddress { get; init; } = string.Empty;
    public List<ShipmentEventResponse> Events { get; init; } = new();
}
