using Microsoft.EntityFrameworkCore;
using Oms.Persistence;
using Oms.Persistence.Entities;
using Shipments.Api.Models;

namespace Shipments.Api.Services;

public sealed class PostalCodePriceListsService
{
    private readonly OmsDbContext _dbContext;

    public PostalCodePriceListsService(OmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<PostalCodePriceListResponse>> GetAllAsync(CancellationToken cancellationToken = default)
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

        return priceLists;
    }

    public async Task<PostalCodePriceListResponse?> GetByIdAsync(Guid postalCodePriceListId, CancellationToken cancellationToken = default)
    {
        var priceList = await _dbContext.PostalCodePriceLists
            .AsNoTracking()
            .SingleOrDefaultAsync(current => current.PostalCodePriceListId == postalCodePriceListId, cancellationToken);

        return priceList is null
            ? null
            : new PostalCodePriceListResponse
            {
                Id = priceList.PostalCodePriceListId,
                ListName = priceList.ListName,
                PostalCode = priceList.PostalCode,
                Zone = priceList.Zone,
                Value = priceList.Value
            };
    }

    public async Task<PostalCodePriceListResponse> CreateAsync(UpsertPostalCodePriceListRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var normalizedPostalCode = NormalizePostalCode(request.PostalCode);
        var normalizedListName = request.ListName.Trim();
        var postalCode = await _dbContext.PostalCodes
            .AsNoTracking()
            .SingleOrDefaultAsync(current => current.PostalCode == normalizedPostalCode, cancellationToken)
            ?? throw new InvalidOperationException("El código postal informado no existe en el catálogo.");

        if (await _dbContext.PostalCodePriceLists.AnyAsync(current => current.ListName == normalizedListName && current.PostalCode == normalizedPostalCode, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe una tarifa para esa lista y código postal.");
        }

        var priceList = new PostalCodePriceListEntity
        {
            PostalCodePriceListId = Guid.NewGuid(),
            ListName = normalizedListName,
            PostalCode = normalizedPostalCode,
            Zone = postalCode.Zone,
            Value = request.Value
        };

        await _dbContext.PostalCodePriceLists.AddAsync(priceList, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new PostalCodePriceListResponse
        {
            Id = priceList.PostalCodePriceListId,
            ListName = priceList.ListName,
            PostalCode = priceList.PostalCode,
            Zone = priceList.Zone,
            Value = priceList.Value
        };
    }

    public async Task<PostalCodePriceListResponse> UpdateAsync(Guid postalCodePriceListId, UpsertPostalCodePriceListRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var normalizedPostalCode = NormalizePostalCode(request.PostalCode);
        var normalizedListName = request.ListName.Trim();
        var postalCode = await _dbContext.PostalCodes
            .AsNoTracking()
            .SingleOrDefaultAsync(current => current.PostalCode == normalizedPostalCode, cancellationToken)
            ?? throw new InvalidOperationException("El código postal informado no existe en el catálogo.");

        var priceList = await _dbContext.PostalCodePriceLists
            .SingleOrDefaultAsync(current => current.PostalCodePriceListId == postalCodePriceListId, cancellationToken)
            ?? throw new KeyNotFoundException("La tarifa no existe.");

        if (await _dbContext.PostalCodePriceLists.AnyAsync(current => current.PostalCodePriceListId != postalCodePriceListId && current.ListName == normalizedListName && current.PostalCode == normalizedPostalCode, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe otra tarifa para esa lista y código postal.");
        }

        priceList.ListName = normalizedListName;
        priceList.PostalCode = normalizedPostalCode;
        priceList.Zone = postalCode.Zone;
        priceList.Value = request.Value;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new PostalCodePriceListResponse
        {
            Id = priceList.PostalCodePriceListId,
            ListName = priceList.ListName,
            PostalCode = priceList.PostalCode,
            Zone = priceList.Zone,
            Value = priceList.Value
        };
    }

    private static void ValidateRequest(UpsertPostalCodePriceListRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ListName) || string.IsNullOrWhiteSpace(request.PostalCode))
        {
            throw new InvalidOperationException("La lista y el código postal son obligatorios.");
        }

        if (request.Value < 0)
        {
            throw new InvalidOperationException("La tarifa no puede ser negativa.");
        }
    }

    private static string NormalizePostalCode(string postalCode)
    {
        return new string((postalCode ?? string.Empty).Trim().Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
    }
}