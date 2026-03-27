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
                existing.PostalCodePrefix = normalizedPrefix;
                existing.BaseCost = ruleRequest.BaseCost;
                existing.UpdatedAt = DateTime.UtcNow;
                continue;
            }

            settings.Rules.Add(new ShipmentPricingRuleEntity
            {
                ShipmentPricingRuleId = Guid.NewGuid(),
                RuleName = ruleRequest.RuleName.Trim(),
                PostalCodePrefix = normalizedPrefix,
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

        return QuoteAsync(request.DestinationPostalCode, request.IncludeInsurance, cancellationToken);
    }

    public async Task<ShipmentPricingQuoteResponse> QuoteAsync(string destinationPostalCode, bool includeInsurance, CancellationToken cancellationToken = default)
    {
        var normalizedPostalCode = NormalizePostalCode(destinationPostalCode);
        if (string.IsNullOrWhiteSpace(normalizedPostalCode))
        {
            throw new InvalidOperationException("El código postal de entrega es obligatorio para cotizar el envío.");
        }

        var settings = await LoadSettingsEntityAsync(cancellationToken);
        var matchedRule = settings.Rules
            .OrderByDescending(current => current.PostalCodePrefix.Length)
            .FirstOrDefault(current => normalizedPostalCode.StartsWith(current.PostalCodePrefix, StringComparison.OrdinalIgnoreCase));

        var baseShippingCost = matchedRule?.BaseCost ?? settings.DefaultBaseCost;
        var insuranceCost = includeInsurance ? settings.InsuranceFlatCost : 0m;

        return new ShipmentPricingQuoteResponse
        {
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
                PostalCodePrefix = current.PostalCodePrefix,
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

        if (request.Rules.Any(current => string.IsNullOrWhiteSpace(current.RuleName) || string.IsNullOrWhiteSpace(current.PostalCodePrefix) || current.BaseCost < 0))
        {
            throw new InvalidOperationException("Cada regla debe incluir nombre, prefijo postal y costo base válido.");
        }

        var normalizedPrefixes = request.Rules
            .Select(current => NormalizePostalCode(current.PostalCodePrefix))
            .ToArray();

        if (normalizedPrefixes.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException("Los prefijos postales deben contener al menos un dígito o carácter válido.");
        }

        if (normalizedPrefixes.Distinct(StringComparer.OrdinalIgnoreCase).Count() != normalizedPrefixes.Length)
        {
            throw new InvalidOperationException("No puede haber reglas duplicadas para el mismo prefijo postal.");
        }
    }

    private static string NormalizePostalCode(string postalCode)
    {
        return new string((postalCode ?? string.Empty).Trim().Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
    }
}