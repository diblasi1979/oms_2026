using Microsoft.EntityFrameworkCore;
using Oms.Persistence;
using Oms.Persistence.Entities;
using Shipments.Api.Models;

namespace Shipments.Api.Services;

public sealed class ShipmentPricingService
{
    private readonly OmsDbContext _dbContext;

    public ShipmentPricingService(OmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ShipmentPricingSettingsResponse> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var priceLists = await _dbContext.PostalCodePriceLists
            .AsNoTracking()
            .OrderBy(current => current.ListName)
            .ThenBy(current => current.PostalCode)
            .Select(current => new PostalCodePriceListResponse
            {
                Id = current.PostalCodePriceListId,
                ListName = current.ListName,
                PostalCode = current.PostalCode,
                Zone = current.Zone,
                Value = current.Value
            })
            .ToListAsync(cancellationToken);

        return new ShipmentPricingSettingsResponse
        {
            PriceLists = priceLists
        };
    }

    public async Task<ShipmentPricingSettingsResponse> UpdateSettingsAsync(UpdateShipmentPricingSettingsRequest request, CancellationToken cancellationToken = default)
    {
        ValidateSettingsRequest(request);

        var normalizedPostalCodes = request.PriceLists
            .Select(current => NormalizePostalCode(current.PostalCode))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var postalCodes = await _dbContext.PostalCodes
            .Where(current => normalizedPostalCodes.Contains(current.PostalCode))
            .ToDictionaryAsync(current => current.PostalCode, StringComparer.OrdinalIgnoreCase, cancellationToken);

        if (postalCodes.Count != normalizedPostalCodes.Length)
        {
            throw new InvalidOperationException("Cada tarifa debe referenciar un código postal existente en el catálogo.");
        }

        var existingEntries = await _dbContext.PostalCodePriceLists
            .ToListAsync(cancellationToken);
        var requestedEntriesById = request.PriceLists
            .Where(current => current.Id.HasValue)
            .ToDictionary(current => current.Id!.Value);

        foreach (var entryToRemove in existingEntries.Where(current => !requestedEntriesById.ContainsKey(current.PostalCodePriceListId)))
        {
            _dbContext.PostalCodePriceLists.Remove(entryToRemove);
        }

        foreach (var entryRequest in request.PriceLists)
        {
            var normalizedPostalCode = NormalizePostalCode(entryRequest.PostalCode);
            var catalogEntry = postalCodes[normalizedPostalCode];
            var normalizedListName = entryRequest.ListName.Trim();

            if (entryRequest.Id.HasValue)
            {
                var existing = existingEntries.Single(current => current.PostalCodePriceListId == entryRequest.Id.Value);
                existing.ListName = normalizedListName;
                existing.PostalCode = normalizedPostalCode;
                existing.Zone = catalogEntry.Zone;
                existing.Value = entryRequest.Value;
                continue;
            }

            await _dbContext.PostalCodePriceLists.AddAsync(new PostalCodePriceListEntity
            {
                PostalCodePriceListId = Guid.NewGuid(),
                ListName = normalizedListName,
                PostalCode = normalizedPostalCode,
                Zone = catalogEntry.Zone,
                Value = entryRequest.Value
            }, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetSettingsAsync(cancellationToken);
    }

    public Task<ShipmentPricingQuoteResponse> QuoteAsync(ShipmentPricingQuoteRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new InvalidOperationException("La cotización requiere datos válidos.");
        }

        if (request.CustomerId == Guid.Empty)
        {
            throw new InvalidOperationException("La cotización requiere un cliente válido.");
        }

        return QuoteByCustomerAsync(request.CustomerId, request.CarrierId, request.DestinationPostalCode, request.DeclaredValue, request.IncludeInsurance, cancellationToken);
    }

    public Task<ShipmentPricingQuoteResponse> QuoteByCustomerAsync(Guid customerId, Guid carrierId, string destinationPostalCode, bool includeInsurance, CancellationToken cancellationToken = default)
    {
        return QuoteByCustomerAsync(customerId, carrierId, destinationPostalCode, 0m, includeInsurance, cancellationToken);
    }

    public async Task<ShipmentPricingQuoteResponse> QuoteByCustomerAsync(Guid customerId, Guid carrierId, string destinationPostalCode, decimal declaredValue, bool includeInsurance, CancellationToken cancellationToken = default)
    {
        var normalizedPostalCode = NormalizePostalCode(destinationPostalCode);
        if (customerId == Guid.Empty || carrierId == Guid.Empty || string.IsNullOrWhiteSpace(normalizedPostalCode))
        {
            throw new InvalidOperationException("La cotización requiere cliente, carrier y código postal válidos.");
        }

        if (declaredValue < 0)
        {
            throw new InvalidOperationException("El valor declarado no puede ser negativo.");
        }

        var carrier = await _dbContext.Carriers
            .AsNoTracking()
            .SingleOrDefaultAsync(current => current.CarrierId == carrierId, cancellationToken)
            ?? throw new InvalidOperationException("El carrier informado no existe.");
        var customer = await _dbContext.Customers
            .AsNoTracking()
            .Include(current => current.CustomerType)
            .SingleOrDefaultAsync(current => current.CustomerId == customerId, cancellationToken)
            ?? throw new InvalidOperationException("El cliente informado no existe.");

        if (!customer.IsActive)
        {
            throw new InvalidOperationException("El cliente informado está inactivo.");
        }

        return await BuildQuoteAsync(
            customer.CustomerId,
            customer.Name,
            customer.CustomerTypeId,
            customer.CustomerType.Code,
            customer.CustomerType.Name,
            customer.AssignedPriceListName,
            customer.InsuranceRatePercentage,
            carrier,
            normalizedPostalCode,
            declaredValue,
            includeInsurance,
            cancellationToken);
    }

    private async Task<ShipmentPricingQuoteResponse> BuildQuoteAsync(
        Guid? customerId,
        string customerName,
        Guid customerTypeId,
        string customerTypeCode,
        string customerTypeName,
        string assignedPriceListName,
        decimal insuranceRatePercentage,
        CarrierEntity carrier,
        string normalizedPostalCode,
        decimal declaredValue,
        bool includeInsurance,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(assignedPriceListName))
        {
            throw new InvalidOperationException(customerId.HasValue
                ? "El cliente no tiene una lista de precios asignada."
                : "El tipo de cliente no tiene una lista de precios asignada.");
        }

        if (includeInsurance && !carrier.InsuranceSupported)
        {
            throw new InvalidOperationException("El carrier seleccionado no admite seguro.");
        }

        var postalCode = await _dbContext.PostalCodes
            .AsNoTracking()
            .SingleOrDefaultAsync(current => current.PostalCode == normalizedPostalCode, cancellationToken)
            ?? throw new InvalidOperationException("El código postal informado no existe en el catálogo.");

        if (!postalCode.IsActive)
        {
            throw new InvalidOperationException("El código postal informado está inactivo.");
        }

        var matchedPrice = await _dbContext.PostalCodePriceLists
            .AsNoTracking()
            .SingleOrDefaultAsync(current => current.ListName == assignedPriceListName && current.PostalCode == normalizedPostalCode, cancellationToken)
            ?? throw new InvalidOperationException("No existe una tarifa para la lista asignada y el código postal informado.");

        var baseShippingCost = matchedPrice.Value;
        var insuranceCost = includeInsurance
            ? decimal.Round(declaredValue * insuranceRatePercentage / 100m, 2, MidpointRounding.AwayFromZero)
            : 0m;

        return new ShipmentPricingQuoteResponse
        {
            CustomerId = customerId,
            CustomerName = customerName,
            CustomerTypeId = customerTypeId,
            CustomerTypeCode = customerTypeCode,
            CustomerTypeName = customerTypeName,
            CarrierId = carrier.CarrierId,
            CarrierName = carrier.Name,
            DestinationPostalCode = normalizedPostalCode,
            AssignedPriceListName = assignedPriceListName,
            MatchedZone = matchedPrice.Zone,
            InsuranceRatePercentage = insuranceRatePercentage,
            DeclaredValue = declaredValue,
            BaseShippingCost = baseShippingCost,
            InsuranceCost = insuranceCost,
            TotalShippingCost = baseShippingCost + insuranceCost
        };
    }

    private static void ValidateSettingsRequest(UpdateShipmentPricingSettingsRequest request)
    {
        if (request.PriceLists.Any(current => string.IsNullOrWhiteSpace(current.ListName) || string.IsNullOrWhiteSpace(current.PostalCode) || current.Value < 0))
        {
            throw new InvalidOperationException("Cada tarifa debe incluir lista, código postal y valor válido.");
        }

        var normalizedKeys = request.PriceLists
            .Select(current => new
            {
                ListName = current.ListName.Trim(),
                PostalCode = NormalizePostalCode(current.PostalCode)
            })
            .ToArray();

        if (normalizedKeys.Any(current => string.IsNullOrWhiteSpace(current.ListName) || string.IsNullOrWhiteSpace(current.PostalCode)))
        {
            throw new InvalidOperationException("Las listas y códigos postales deben contener valores válidos.");
        }

        if (normalizedKeys.DistinctBy(current => $"{current.ListName}|{current.PostalCode}", StringComparer.OrdinalIgnoreCase).Count() != normalizedKeys.Length)
        {
            throw new InvalidOperationException("No puede haber dos tarifas para la misma combinación de lista y código postal.");
        }
    }

    private static string NormalizePostalCode(string postalCode)
    {
        return new string((postalCode ?? string.Empty).Trim().Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
    }
}