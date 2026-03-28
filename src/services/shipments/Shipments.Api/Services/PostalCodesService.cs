using Microsoft.EntityFrameworkCore;
using Oms.Persistence;
using Oms.Persistence.Entities;
using Shipments.Api.Models;

namespace Shipments.Api.Services;

public sealed class PostalCodesService
{
    private readonly OmsDbContext _dbContext;

    public PostalCodesService(OmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<PostalCodeResponse>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.PostalCodes.AsNoTracking().AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(current => current.IsActive);
        }

        var postalCodes = await query
            .OrderByDescending(current => current.IsActive)
            .ThenBy(current => current.Country)
            .ThenBy(current => current.Province)
            .ThenBy(current => current.Locality)
            .ThenBy(current => current.PostalCode)
            .ToListAsync(cancellationToken);

        return postalCodes.Select(Map).ToArray();
    }

    public async Task<PostalCodeResponse?> GetByIdAsync(Guid postalCodeId, CancellationToken cancellationToken = default)
    {
        var postalCode = await _dbContext.PostalCodes
            .AsNoTracking()
            .SingleOrDefaultAsync(current => current.PostalCodeId == postalCodeId, cancellationToken);

        return postalCode is null ? null : Map(postalCode);
    }

    public async Task<PostalCodeResponse> CreateAsync(UpsertPostalCodeRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var normalizedPostalCode = NormalizePostalCode(request.PostalCode);
        var normalizedCountry = request.Country.Trim();
        var normalizedProvince = request.Province.Trim();
        var normalizedLocality = request.Locality.Trim();

        if (await _dbContext.PostalCodes.AnyAsync(current =>
            current.Country == normalizedCountry &&
            current.Province == normalizedProvince &&
            current.Locality == normalizedLocality &&
            current.PostalCode == normalizedPostalCode, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe un código postal con el mismo país, provincia, localidad y código.");
        }

        var postalCode = new PostalCodeEntity
        {
            PostalCodeId = Guid.NewGuid(),
            Country = normalizedCountry,
            Province = normalizedProvince,
            Locality = normalizedLocality,
            PostalCode = normalizedPostalCode,
            IsActive = request.IsActive,
            Zone = request.Zone.Trim()
        };

        await _dbContext.PostalCodes.AddAsync(postalCode, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(postalCode);
    }

    public async Task<PostalCodeResponse> UpdateAsync(Guid postalCodeId, UpsertPostalCodeRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var normalizedPostalCode = NormalizePostalCode(request.PostalCode);
        var normalizedCountry = request.Country.Trim();
        var normalizedProvince = request.Province.Trim();
        var normalizedLocality = request.Locality.Trim();
        var normalizedZone = request.Zone.Trim();

        var postalCode = await _dbContext.PostalCodes
            .SingleOrDefaultAsync(current => current.PostalCodeId == postalCodeId, cancellationToken)
            ?? throw new KeyNotFoundException("El código postal no existe.");

        if (await _dbContext.PostalCodes.AnyAsync(current =>
            current.PostalCodeId != postalCodeId &&
            current.Country == normalizedCountry &&
            current.Province == normalizedProvince &&
            current.Locality == normalizedLocality &&
            current.PostalCode == normalizedPostalCode, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe otro código postal con el mismo país, provincia, localidad y código.");
        }

        var previousPostalCode = postalCode.PostalCode;
        postalCode.Country = normalizedCountry;
        postalCode.Province = normalizedProvince;
        postalCode.Locality = normalizedLocality;
        postalCode.PostalCode = normalizedPostalCode;
        postalCode.Zone = normalizedZone;
        postalCode.IsActive = request.IsActive;

        var relatedPriceLists = await _dbContext.PostalCodePriceLists
            .Where(current => current.PostalCode == previousPostalCode)
            .ToListAsync(cancellationToken);

        foreach (var priceList in relatedPriceLists)
        {
            priceList.PostalCode = normalizedPostalCode;
            priceList.Zone = normalizedZone;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(postalCode);
    }

    private static PostalCodeResponse Map(PostalCodeEntity postalCode) => new()
    {
        Id = postalCode.PostalCodeId,
        Country = postalCode.Country,
        Province = postalCode.Province,
        Locality = postalCode.Locality,
        PostalCode = postalCode.PostalCode,
        IsActive = postalCode.IsActive,
        Zone = postalCode.Zone
    };

    private static void ValidateRequest(UpsertPostalCodeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Country) || string.IsNullOrWhiteSpace(request.Province) || string.IsNullOrWhiteSpace(request.Locality) || string.IsNullOrWhiteSpace(request.PostalCode) || string.IsNullOrWhiteSpace(request.Zone))
        {
            throw new InvalidOperationException("País, provincia, localidad, código postal y zona son obligatorios.");
        }
    }

    private static string NormalizePostalCode(string postalCode)
    {
        return new string((postalCode ?? string.Empty).Trim().Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
    }
}