using Microsoft.EntityFrameworkCore;
using Oms.Persistence;
using Oms.Persistence.Entities;
using Orders.Api.Models;

namespace Orders.Api.Services;

public sealed class CustomerTypesService
{
    private readonly OmsDbContext _dbContext;

    public CustomerTypesService(OmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<CustomerTypeResponse>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.CustomerTypes.AsNoTracking().AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(current => current.IsActive);
        }

        var customerTypes = await query
            .OrderByDescending(current => current.IsActive)
            .ThenBy(current => current.Name)
            .ToListAsync(cancellationToken);

        return customerTypes.Select(Map).ToArray();
    }

    public async Task<CustomerTypeResponse?> GetByIdAsync(Guid customerTypeId, CancellationToken cancellationToken = default)
    {
        var customerType = await _dbContext.CustomerTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(current => current.CustomerTypeId == customerTypeId, cancellationToken);

        return customerType is null ? null : Map(customerType);
    }

    public async Task<CustomerTypeResponse> CreateAsync(UpsertCustomerTypeRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);
        var normalizedCode = NormalizeCode(request.Code);

        if (await _dbContext.CustomerTypes.AnyAsync(current => current.Code == normalizedCode, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe un tipo de cliente con el mismo código.");
        }

        var customerType = new CustomerTypeEntity
        {
            CustomerTypeId = Guid.NewGuid(),
            Code = normalizedCode,
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            AssignedPriceListName = request.AssignedPriceListName.Trim(),
            InsuranceRatePercentage = request.InsuranceRatePercentage,
            IsActive = request.IsActive,
            UpdatedAt = DateTime.UtcNow
        };

        await _dbContext.CustomerTypes.AddAsync(customerType, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(customerType);
    }

    public async Task<CustomerTypeResponse> UpdateAsync(Guid customerTypeId, UpsertCustomerTypeRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);
        var normalizedCode = NormalizeCode(request.Code);

        var customerType = await _dbContext.CustomerTypes
            .SingleOrDefaultAsync(current => current.CustomerTypeId == customerTypeId, cancellationToken)
            ?? throw new KeyNotFoundException("El tipo de cliente no existe.");

        if (await _dbContext.CustomerTypes.AnyAsync(current => current.CustomerTypeId != customerTypeId && current.Code == normalizedCode, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe otro tipo de cliente con el mismo código.");
        }

        customerType.Code = normalizedCode;
        customerType.Name = request.Name.Trim();
        customerType.Description = request.Description.Trim();
        customerType.AssignedPriceListName = request.AssignedPriceListName.Trim();
        customerType.InsuranceRatePercentage = request.InsuranceRatePercentage;
        customerType.IsActive = request.IsActive;
        customerType.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(customerType);
    }

    private static CustomerTypeResponse Map(CustomerTypeEntity customerType) => new()
    {
        Id = customerType.CustomerTypeId,
        Code = customerType.Code,
        Name = customerType.Name,
        Description = customerType.Description,
        AssignedPriceListName = customerType.AssignedPriceListName,
        InsuranceRatePercentage = customerType.InsuranceRatePercentage,
        IsActive = customerType.IsActive
    };

    private static void ValidateRequest(UpsertCustomerTypeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.AssignedPriceListName))
        {
            throw new InvalidOperationException("Código, nombre y lista de precios asignada son obligatorios.");
        }

        if (request.InsuranceRatePercentage < 0)
        {
            throw new InvalidOperationException("El porcentaje de seguro no puede ser negativo.");
        }
    }

    private static string NormalizeCode(string code)
    {
        return new string((code ?? string.Empty).Trim().Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
    }
}