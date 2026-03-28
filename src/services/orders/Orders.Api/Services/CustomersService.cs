using Microsoft.EntityFrameworkCore;
using Oms.Persistence;
using Oms.Persistence.Entities;
using Orders.Api.Models;

namespace Orders.Api.Services;

public sealed class CustomersService
{
    private readonly OmsDbContext _dbContext;

    public CustomersService(OmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<CustomerResponse>> GetAllAsync(bool includeInactive, string? search, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Customers
            .AsNoTracking()
            .Include(current => current.CustomerType)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(current => current.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(current =>
                current.Name.Contains(normalizedSearch) ||
                current.Code.Contains(normalizedSearch) ||
                current.CustomerType.Name.Contains(normalizedSearch));
        }

        var customers = await query
            .OrderByDescending(current => current.IsActive)
            .ThenBy(current => current.Name)
            .ToListAsync(cancellationToken);

        return customers.Select(Map).ToArray();
    }

    public async Task<CustomerResponse?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _dbContext.Customers
            .AsNoTracking()
            .Include(current => current.CustomerType)
            .SingleOrDefaultAsync(current => current.CustomerId == customerId, cancellationToken);

        return customer is null ? null : Map(customer);
    }

    public async Task<CustomerResponse> CreateAsync(UpsertCustomerRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var customerType = await _dbContext.CustomerTypes
            .SingleOrDefaultAsync(current => current.CustomerTypeId == request.CustomerTypeId, cancellationToken)
            ?? throw new KeyNotFoundException("El tipo de cliente no existe.");

        if (await _dbContext.Customers.AnyAsync(current => current.CustomerTypeId == request.CustomerTypeId && current.Name == request.Name.Trim(), cancellationToken))
        {
            throw new InvalidOperationException("Ya existe un cliente con el mismo nombre para ese tipo.");
        }

        var customer = new CustomerEntity
        {
            CustomerId = Guid.NewGuid(),
            Code = await CustomerCodeGenerator.GenerateUniqueCodeAsync(_dbContext, request.Code, request.Name, null, cancellationToken),
            Name = request.Name.Trim(),
            CustomerTypeId = request.CustomerTypeId,
            AssignedPriceListName = request.AssignedPriceListName.Trim(),
            InsuranceRatePercentage = request.InsuranceRatePercentage,
            IsActive = request.IsActive,
            UpdatedAt = DateTime.UtcNow,
            CustomerType = customerType
        };

        await _dbContext.Customers.AddAsync(customer, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(customer);
    }

    public async Task<CustomerResponse> UpdateAsync(Guid customerId, UpsertCustomerRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var customer = await _dbContext.Customers
            .Include(current => current.CustomerType)
            .SingleOrDefaultAsync(current => current.CustomerId == customerId, cancellationToken)
            ?? throw new KeyNotFoundException("El cliente no existe.");

        var customerType = await _dbContext.CustomerTypes
            .SingleOrDefaultAsync(current => current.CustomerTypeId == request.CustomerTypeId, cancellationToken)
            ?? throw new KeyNotFoundException("El tipo de cliente no existe.");

        if (await _dbContext.Customers.AnyAsync(current => current.CustomerId != customerId && current.CustomerTypeId == request.CustomerTypeId && current.Name == request.Name.Trim(), cancellationToken))
        {
            throw new InvalidOperationException("Ya existe otro cliente con el mismo nombre para ese tipo.");
        }

        customer.Code = await CustomerCodeGenerator.GenerateUniqueCodeAsync(_dbContext, request.Code, request.Name, customerId, cancellationToken);
        customer.Name = request.Name.Trim();
        customer.CustomerTypeId = request.CustomerTypeId;
        customer.AssignedPriceListName = request.AssignedPriceListName.Trim();
        customer.InsuranceRatePercentage = request.InsuranceRatePercentage;
        customer.IsActive = request.IsActive;
        customer.UpdatedAt = DateTime.UtcNow;
        customer.CustomerType = customerType;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(customer);
    }

    private static CustomerResponse Map(CustomerEntity customer) => new()
    {
        Id = customer.CustomerId,
        Code = customer.Code,
        Name = customer.Name,
        CustomerTypeId = customer.CustomerTypeId,
        CustomerTypeCode = customer.CustomerType.Code,
        CustomerTypeName = customer.CustomerType.Name,
        AssignedPriceListName = customer.AssignedPriceListName,
        InsuranceRatePercentage = customer.InsuranceRatePercentage,
        IsActive = customer.IsActive
    };

    private static void ValidateRequest(UpsertCustomerRequest request)
    {
        if (request.CustomerTypeId == Guid.Empty || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.AssignedPriceListName))
        {
            throw new InvalidOperationException("Tipo de cliente, nombre y lista de precios asignada son obligatorios.");
        }

        if (request.InsuranceRatePercentage < 0)
        {
            throw new InvalidOperationException("El porcentaje de seguro no puede ser negativo.");
        }
    }
}