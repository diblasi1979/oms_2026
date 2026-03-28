using Microsoft.EntityFrameworkCore;
using Oms.Persistence;

namespace Orders.Api.Services;

internal static class CustomerCodeGenerator
{
    public static async Task<string> GenerateUniqueCodeAsync(
        OmsDbContext dbContext,
        string requestedCode,
        string fallbackName,
        Guid? currentCustomerId,
        CancellationToken cancellationToken)
    {
        var baseCode = NormalizeCode(string.IsNullOrWhiteSpace(requestedCode) ? fallbackName : requestedCode);
        if (string.IsNullOrWhiteSpace(baseCode))
        {
            baseCode = "CUSTOMER";
        }

        var suffix = 0;
        while (true)
        {
            var candidate = suffix == 0 ? baseCode : $"{baseCode}{suffix:D2}";
            var exists = currentCustomerId.HasValue
                ? await dbContext.Customers.AnyAsync(
                    current => current.CustomerId != currentCustomerId.Value && current.Code == candidate,
                    cancellationToken)
                : await dbContext.Customers.AnyAsync(
                    current => current.Code == candidate,
                    cancellationToken);

            if (!exists)
            {
                return candidate;
            }

            suffix++;
        }
    }

    public static string NormalizeCode(string value)
    {
        return new string((value ?? string.Empty).Trim().Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
    }
}