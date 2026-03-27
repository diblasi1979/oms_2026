namespace Orders.Api.Configuration;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "OMS.Auth";
    public string Audience { get; set; } = "OMS.Clients";
    public string Key { get; set; } = string.Empty;
}
