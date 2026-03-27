using System.Text.Json;

namespace Integrations.Api.Models;

public enum ExternalProvider
{
    Shopify,
    MercadoLibre,
    WooCommerce,
    Amazon
}

public sealed class CanonicalExternalOrderResponse
{
    public Guid IntegrationEventId { get; init; }
    public ExternalProvider Provider { get; init; }
    public string ExternalOrderId { get; init; } = string.Empty;
    public string Customer { get; init; } = string.Empty;
    public string DestinationCity { get; init; } = string.Empty;
    public string DestinationState { get; init; } = string.Empty;
    public string DestinationPostalCode { get; init; } = string.Empty;
    public string ShipmentMode { get; init; } = "Standard";
    public List<CanonicalExternalOrderItem> Items { get; init; } = new();
    public DateTimeOffset ReceivedAt { get; init; }
    public JsonElement RawPayload { get; init; }
}

public sealed class CanonicalExternalOrderItem
{
    public string Sku { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}
