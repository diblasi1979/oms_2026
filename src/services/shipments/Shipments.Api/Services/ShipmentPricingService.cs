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
        var settings = await LoadSettingsEntityAsync(cancellationToken);
        return Map(settings);
    }

    public async Task<ShipmentPricingSettingsResponse> UpdateSettingsAsync(UpdateShipmentPricingSettingsRequest request, CancellationToken cancellationToken = default)
    {
        ValidateSettingsRequest(request);

        var settings = await _dbContext.ShipmentPricingSettings
            .Include(current => current.Rules)
            .SingleAsync(current => current.ShipmentPricingSettingsId == 1, cancellationToken);

        var carrierIds = request.Rules.Select(current => current.CarrierId).Distinct().ToArray();
        var customerTypeIds = request.Rules.Select(current => current.CustomerTypeId).Distinct().ToArray();
        var existingCarrierIds = await _dbContext.Carriers
            .Where(current => carrierIds.Contains(current.CarrierId))
            .Select(current => current.CarrierId)
            .ToListAsync(cancellationToken);
        var existingCustomerTypeIds = await _dbContext.CustomerTypes
            .Where(current => customerTypeIds.Contains(current.CustomerTypeId))
            .Select(current => current.CustomerTypeId)
            .ToListAsync(cancellationToken);

        if (existingCarrierIds.Count != carrierIds.Length)
        {
            throw new InvalidOperationException("Cada regla debe referenciar un carrier existente.");
        }

        if (existingCustomerTypeIds.Count != customerTypeIds.Length)
        {
            throw new InvalidOperationException("Cada regla debe referenciar un tipo de cliente existente.");
        }

        settings.DefaultBaseCost = request.DefaultBaseCost;
        settings.InsuranceFlatCost = request.InsuranceFlatCost;
        settings.UpdatedAt = DateTime.UtcNow;

        var requestedRulesById = request.Rules.Where(current => current.Id.HasValue)
            .ToDictionary(current => current.Id!.Value);

        var rulesToRemove = settings.Rules.Where(current => !requestedRulesById.ContainsKey(current.ShipmentPricingRuleId)).ToList();
        foreach (var rule in rulesToRemove)
        {
            _dbContext.ShipmentPricingRules.Remove(rule);
        }

        foreach (var ruleRequest in request.Rules)
        {
            var normalizedPrefix = NormalizePostalCode(ruleRequest.PostalCodePrefix);
            if (ruleRequest.Id.HasValue)
            {
                var existing = settings.Rules.Single(current => current.ShipmentPricingRuleId == ruleRequest.Id.Value);
                existing.RuleName = ruleRequest.RuleName.Trim();
                existing.CustomerTypeId = ruleRequest.CustomerTypeId;
                existing.PostalCodePrefix = normalizedPrefix;
                existing.CarrierId = ruleRequest.CarrierId;
                existing.BaseCost = ruleRequest.BaseCost;
                existing.UpdatedAt = DateTime.UtcNow;
                continue;
            }

            settings.Rules.Add(new ShipmentPricingRuleEntity
            {
                ShipmentPricingRuleId = Guid.NewGuid(),
                RuleName = ruleRequest.RuleName.Trim(),
                CustomerTypeId = ruleRequest.CustomerTypeId,
                PostalCodePrefix = normalizedPrefix,
                CarrierId = ruleRequest.CarrierId,
                BaseCost = ruleRequest.BaseCost,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(settings);
    }

    public Task<ShipmentPricingQuoteResponse> QuoteAsync(ShipmentPricingQuoteRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new InvalidOperationException("La cotización requiere datos válidos.");
        }

        return QuoteAsync(request.CustomerTypeId, request.CarrierId, request.DestinationPostalCode, request.IncludeInsurance, cancellationToken);
    }

    public async Task<ShipmentPricingQuoteResponse> QuoteAsync(Guid customerTypeId, Guid carrierId, string destinationPostalCode, bool includeInsurance, CancellationToken cancellationToken = default)
    {
        var normalizedPostalCode = NormalizePostalCode(destinationPostalCode);
        if (customerTypeId == Guid.Empty || carrierId == Guid.Empty || string.IsNullOrWhiteSpace(normalizedPostalCode))
        {
            throw new InvalidOperationException("La cotización requiere tipo de cliente, carrier y código postal válidos.");
        }

        var settings = await LoadSettingsEntityAsync(cancellationToken);
        var carrier = await _dbContext.Carriers
            .AsNoTracking()
            .SingleOrDefaultAsync(current => current.CarrierId == carrierId, cancellationToken)
            ?? throw new InvalidOperationException("El carrier informado no existe.");
        var customerType = await _dbContext.CustomerTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(current => current.CustomerTypeId == customerTypeId, cancellationToken)
            ?? throw new InvalidOperationException("El tipo de cliente informado no existe.");

        var matchedRule = settings.Rules
            .Where(current => current.CarrierId == carrierId && current.CustomerTypeId == customerTypeId)
            .OrderByDescending(current => current.PostalCodePrefix.Length)
            .FirstOrDefault(current => normalizedPostalCode.StartsWith(current.PostalCodePrefix, StringComparison.OrdinalIgnoreCase));

        var baseShippingCost = matchedRule?.BaseCost ?? settings.DefaultBaseCost;
        var insuranceCost = includeInsurance ? settings.InsuranceFlatCost : 0m;

        return new ShipmentPricingQuoteResponse
        {
            CustomerTypeId = customerTypeId,
            CustomerTypeCode = customerType.Code,
            CustomerTypeName = customerType.Name,
            CarrierId = carrierId,
            CarrierName = carrier.Name,
            DestinationPostalCode = normalizedPostalCode,
            MatchedRuleName = matchedRule?.RuleName,
            MatchedPostalCodePrefix = matchedRule?.PostalCodePrefix,
            UsedDefaultRate = matchedRule is null,
            BaseShippingCost = baseShippingCost,
            InsuranceCost = insuranceCost,
            TotalShippingCost = baseShippingCost + insuranceCost
        };
    }

    private async Task<ShipmentPricingSettingsEntity> LoadSettingsEntityAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.ShipmentPricingSettings
            .AsNoTracking()
            .Include(current => current.Rules)
            .ThenInclude(current => current.CustomerType)
            .Include(current => current.Rules)
            .ThenInclude(current => current.Carrier)
            .SingleAsync(current => current.ShipmentPricingSettingsId == 1, cancellationToken);
    }

    private static ShipmentPricingSettingsResponse Map(ShipmentPricingSettingsEntity settings) => new()
    {
        DefaultBaseCost = settings.DefaultBaseCost,
        InsuranceFlatCost = settings.InsuranceFlatCost,
        Rules = settings.Rules
            .OrderBy(current => current.PostalCodePrefix)
            .Select(current => new ShipmentPricingRuleResponse
            {
                Id = current.ShipmentPricingRuleId,
                RuleName = current.RuleName,
                CustomerTypeId = current.CustomerTypeId,
                CustomerTypeCode = current.CustomerType.Code,
                CustomerTypeName = current.CustomerType.Name,
                PostalCodePrefix = current.PostalCodePrefix,
                CarrierId = current.CarrierId,
                CarrierName = current.Carrier.Name,
                BaseCost = current.BaseCost
            })
            .ToList()
    };

    private static void ValidateSettingsRequest(UpdateShipmentPricingSettingsRequest request)
    {
        if (request.DefaultBaseCost < 0 || request.InsuranceFlatCost < 0)
        {
            throw new InvalidOperationException("Los costos configurables no pueden ser negativos.");
        }

        if (request.Rules.Any(current => string.IsNullOrWhiteSpace(current.RuleName) || current.CustomerTypeId == Guid.Empty || string.IsNullOrWhiteSpace(current.PostalCodePrefix) || current.CarrierId == Guid.Empty || current.BaseCost < 0))
        {
            throw new InvalidOperationException("Cada regla debe incluir nombre, tipo de cliente, carrier, prefijo postal y costo base válido.");
        }

        var normalizedKeys = request.Rules
            .Select(current => new
            {
                current.CustomerTypeId,
                PostalCodePrefix = NormalizePostalCode(current.PostalCodePrefix),
                current.CarrierId
            })
            .ToArray();

        if (normalizedKeys.Any(current => string.IsNullOrWhiteSpace(current.PostalCodePrefix)))
        {
            throw new InvalidOperationException("Los tipos de cliente y prefijos postales deben contener valores válidos.");
        }

        if (normalizedKeys.DistinctBy(current => $"{current.CustomerTypeId}|{current.PostalCodePrefix}|{current.CarrierId}", StringComparer.OrdinalIgnoreCase).Count() != normalizedKeys.Length)
        {
            throw new InvalidOperationException("No puede haber reglas duplicadas para la misma combinación de tipo de cliente, carrier y prefijo postal.");
        }
    }

    private static string NormalizePostalCode(string postalCode)
    {
        return new string((postalCode ?? string.Empty).Trim().Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
    }
}