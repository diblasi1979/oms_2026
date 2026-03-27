using Microsoft.EntityFrameworkCore;
using Oms.Persistence;
using Oms.Persistence.Entities;
using Shipments.Api.Models;

namespace Shipments.Api.Services;

public sealed class CarriersService
{
    private readonly OmsDbContext _dbContext;

    public CarriersService(OmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<CarrierResponse>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Carriers.AsNoTracking().AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(current => current.IsActive);
        }

        var carriers = await query
            .OrderByDescending(current => current.IsActive)
            .ThenBy(current => current.Name)
            .ToListAsync(cancellationToken);

        return carriers.Select(Map).ToArray();
    }

    public async Task<CarrierResponse?> GetByIdAsync(Guid carrierId, CancellationToken cancellationToken = default)
    {
        var carrier = await _dbContext.Carriers
            .AsNoTracking()
            .SingleOrDefaultAsync(current => current.CarrierId == carrierId, cancellationToken);

        return carrier is null ? null : Map(carrier);
    }

    public async Task<CarrierResponse> CreateAsync(UpsertCarrierRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);
        var normalizedCode = NormalizeCode(request.Code);

        if (await _dbContext.Carriers.AnyAsync(current => current.Code == normalizedCode, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe un carrier con el mismo código.");
        }

        var carrier = new CarrierEntity
        {
            CarrierId = Guid.NewGuid(),
            Code = normalizedCode,
            Name = request.Name.Trim(),
            ServiceLevel = request.ServiceLevel.Trim(),
            TrackingUrlTemplate = request.TrackingUrlTemplate.Trim(),
            SupportEmail = request.SupportEmail.Trim(),
            SupportPhone = request.SupportPhone.Trim(),
            InsuranceSupported = request.InsuranceSupported,
            IsActive = request.IsActive,
            Notes = request.Notes.Trim(),
            UpdatedAt = DateTime.UtcNow
        };

        await _dbContext.Carriers.AddAsync(carrier, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(carrier);
    }

    public async Task<CarrierResponse> UpdateAsync(Guid carrierId, UpsertCarrierRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);
        var normalizedCode = NormalizeCode(request.Code);

        var carrier = await _dbContext.Carriers
            .SingleOrDefaultAsync(current => current.CarrierId == carrierId, cancellationToken)
            ?? throw new KeyNotFoundException("El carrier no existe.");

        if (await _dbContext.Carriers.AnyAsync(current => current.CarrierId != carrierId && current.Code == normalizedCode, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe otro carrier con el mismo código.");
        }

        carrier.Code = normalizedCode;
        carrier.Name = request.Name.Trim();
        carrier.ServiceLevel = request.ServiceLevel.Trim();
        carrier.TrackingUrlTemplate = request.TrackingUrlTemplate.Trim();
        carrier.SupportEmail = request.SupportEmail.Trim();
        carrier.SupportPhone = request.SupportPhone.Trim();
        carrier.InsuranceSupported = request.InsuranceSupported;
        carrier.IsActive = request.IsActive;
        carrier.Notes = request.Notes.Trim();
        carrier.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(carrier);
    }

    private static CarrierResponse Map(CarrierEntity carrier) => new()
    {
        Id = carrier.CarrierId,
        Code = carrier.Code,
        Name = carrier.Name,
        ServiceLevel = carrier.ServiceLevel,
        TrackingUrlTemplate = carrier.TrackingUrlTemplate,
        SupportEmail = carrier.SupportEmail,
        SupportPhone = carrier.SupportPhone,
        InsuranceSupported = carrier.InsuranceSupported,
        IsActive = carrier.IsActive,
        Notes = carrier.Notes
    };

    private static void ValidateRequest(UpsertCarrierRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.ServiceLevel))
        {
            throw new InvalidOperationException("Código, nombre y nivel de servicio son obligatorios.");
        }

        if (string.IsNullOrWhiteSpace(request.TrackingUrlTemplate))
        {
            throw new InvalidOperationException("La URL de tracking es obligatoria.");
        }
    }

    private static string NormalizeCode(string code)
    {
        return new string((code ?? string.Empty).Trim().Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
    }
}
