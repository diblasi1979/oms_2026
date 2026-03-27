using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Integrations.Api.Models;
using Oms.Persistence;
using Oms.Persistence.Entities;

namespace Integrations.Api.Services;

public sealed class ExternalOrdersService
{
    private readonly OmsDbContext _dbContext;

    public ExternalOrdersService(OmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CanonicalExternalOrderResponse> RegisterAsync(ExternalProvider provider, JsonElement payload, CancellationToken cancellationToken = default)
    {
        var normalized = provider switch
        {
            ExternalProvider.Shopify => MapShopify(payload),
            ExternalProvider.MercadoLibre => MapMercadoLibre(payload),
            ExternalProvider.WooCommerce => MapWooCommerce(payload),
            ExternalProvider.Amazon => MapAmazon(payload),
            _ => throw new InvalidOperationException("Proveedor no soportado.")
        };

        var existing = await _dbContext.ExternalWebhookEvents
            .AsNoTracking()
            .SingleOrDefaultAsync(current => current.Provider == provider.ToString() && current.ExternalOrderId == normalized.ExternalOrderId, cancellationToken);

        if (existing is not null)
        {
            return JsonSerializer.Deserialize<CanonicalExternalOrderResponse>(existing.CanonicalSnapshot)
                ?? throw new InvalidOperationException("No se pudo reconstruir el webhook persistido.");
        }

        var entity = new ExternalWebhookEventEntity
        {
            ExternalWebhookEventId = normalized.IntegrationEventId,
            Provider = provider.ToString(),
            ExternalOrderId = normalized.ExternalOrderId,
            Payload = payload.GetRawText(),
            CanonicalSnapshot = JsonSerializer.Serialize(normalized),
            ReceivedAt = normalized.ReceivedAt.UtcDateTime,
            ProcessedAt = DateTime.UtcNow
        };

        await _dbContext.ExternalWebhookEvents.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return normalized;
    }

    private static CanonicalExternalOrderResponse MapShopify(JsonElement payload)
    {
        var lineItems = payload.GetProperty("line_items").EnumerateArray().Select(item => new CanonicalExternalOrderItem
        {
            Sku = item.GetProperty("sku").GetString() ?? "UNKNOWN-SKU",
            Quantity = item.GetProperty("quantity").GetInt32(),
            UnitPrice = decimal.Parse(item.GetProperty("price").GetString() ?? "0", CultureInfo.InvariantCulture)
        }).ToList();

        return new CanonicalExternalOrderResponse
        {
            IntegrationEventId = Guid.NewGuid(),
            Provider = ExternalProvider.Shopify,
            ExternalOrderId = payload.GetProperty("id").ToString(),
            Customer = payload.GetProperty("customer").GetProperty("first_name").GetString() + " " + payload.GetProperty("customer").GetProperty("last_name").GetString(),
            DestinationCity = payload.GetProperty("shipping_address").GetProperty("city").GetString() ?? string.Empty,
            DestinationState = payload.GetProperty("shipping_address").GetProperty("province").GetString() ?? string.Empty,
            DestinationPostalCode = payload.GetProperty("shipping_address").GetProperty("zip").GetString() ?? string.Empty,
            ShipmentMode = payload.TryGetProperty("shipping_lines", out var shippingLines) && shippingLines.GetArrayLength() > 0 ? shippingLines[0].GetProperty("title").GetString() ?? "Standard" : "Standard",
            Items = lineItems,
            ReceivedAt = DateTimeOffset.UtcNow,
            RawPayload = payload.Clone()
        };
    }

    private static CanonicalExternalOrderResponse MapMercadoLibre(JsonElement payload)
    {
        var items = payload.GetProperty("order_items").EnumerateArray().Select(item => new CanonicalExternalOrderItem
        {
            Sku = item.GetProperty("item").GetProperty("seller_sku").GetString() ?? item.GetProperty("item").GetProperty("id").GetString() ?? "UNKNOWN-SKU",
            Quantity = item.GetProperty("quantity").GetInt32(),
            UnitPrice = item.GetProperty("unit_price").GetDecimal()
        }).ToList();

        return new CanonicalExternalOrderResponse
        {
            IntegrationEventId = Guid.NewGuid(),
            Provider = ExternalProvider.MercadoLibre,
            ExternalOrderId = payload.GetProperty("id").ToString(),
            Customer = payload.GetProperty("buyer").GetProperty("nickname").GetString() ?? "Marketplace Buyer",
            DestinationCity = payload.GetProperty("shipping").GetProperty("receiver_address").GetProperty("city").GetProperty("name").GetString() ?? string.Empty,
            DestinationState = payload.GetProperty("shipping").GetProperty("receiver_address").GetProperty("state").GetProperty("name").GetString() ?? string.Empty,
            DestinationPostalCode = payload.GetProperty("shipping").GetProperty("receiver_address").GetProperty("zip_code").GetString() ?? string.Empty,
            ShipmentMode = payload.GetProperty("shipping").GetProperty("shipping_mode").GetString() ?? "Standard",
            Items = items,
            ReceivedAt = DateTimeOffset.UtcNow,
            RawPayload = payload.Clone()
        };
    }

    private static CanonicalExternalOrderResponse MapWooCommerce(JsonElement payload)
    {
        var items = payload.GetProperty("line_items").EnumerateArray().Select(item => new CanonicalExternalOrderItem
        {
            Sku = item.TryGetProperty("sku", out var sku) ? sku.GetString() ?? "UNKNOWN-SKU" : "UNKNOWN-SKU",
            Quantity = item.GetProperty("quantity").GetInt32(),
            UnitPrice = decimal.Parse(item.GetProperty("price").ToString(), CultureInfo.InvariantCulture)
        }).ToList();

        return new CanonicalExternalOrderResponse
        {
            IntegrationEventId = Guid.NewGuid(),
            Provider = ExternalProvider.WooCommerce,
            ExternalOrderId = payload.GetProperty("id").ToString(),
            Customer = payload.GetProperty("billing").GetProperty("first_name").GetString() + " " + payload.GetProperty("billing").GetProperty("last_name").GetString(),
            DestinationCity = payload.GetProperty("shipping").GetProperty("city").GetString() ?? string.Empty,
            DestinationState = payload.GetProperty("shipping").GetProperty("state").GetString() ?? string.Empty,
            DestinationPostalCode = payload.GetProperty("shipping").GetProperty("postcode").GetString() ?? string.Empty,
            ShipmentMode = "Standard",
            Items = items,
            ReceivedAt = DateTimeOffset.UtcNow,
            RawPayload = payload.Clone()
        };
    }

    private static CanonicalExternalOrderResponse MapAmazon(JsonElement payload)
    {
        var items = payload.GetProperty("items").EnumerateArray().Select(item => new CanonicalExternalOrderItem
        {
            Sku = item.GetProperty("sellerSku").GetString() ?? "UNKNOWN-SKU",
            Quantity = item.GetProperty("quantityOrdered").GetInt32(),
            UnitPrice = item.GetProperty("itemPrice").GetProperty("amount").GetDecimal()
        }).ToList();

        return new CanonicalExternalOrderResponse
        {
            IntegrationEventId = Guid.NewGuid(),
            Provider = ExternalProvider.Amazon,
            ExternalOrderId = payload.GetProperty("amazonOrderId").GetString() ?? Guid.NewGuid().ToString("N"),
            Customer = payload.GetProperty("buyerInfo").GetProperty("buyerName").GetString() ?? "Amazon Buyer",
            DestinationCity = payload.GetProperty("shippingAddress").GetProperty("city").GetString() ?? string.Empty,
            DestinationState = payload.GetProperty("shippingAddress").GetProperty("stateOrRegion").GetString() ?? string.Empty,
            DestinationPostalCode = payload.GetProperty("shippingAddress").GetProperty("postalCode").GetString() ?? string.Empty,
            ShipmentMode = payload.TryGetProperty("shipmentServiceLevelCategory", out var category) ? category.GetString() ?? "Standard" : "Standard",
            Items = items,
            ReceivedAt = DateTimeOffset.UtcNow,
            RawPayload = payload.Clone()
        };
    }
}
