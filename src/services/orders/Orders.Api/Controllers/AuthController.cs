using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Orders.Api.Configuration;

namespace Orders.Api.Controllers;

public sealed class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly JwtOptions _jwtOptions;

    public AuthController(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    [HttpPost("token")]
    public ActionResult<object> Login([FromBody] LoginRequest request)
    {
        if (!IsValidUser(request))
        {
            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, request.Username),
            new Claim(ClaimTypes.Name, request.Username),
            new Claim(ClaimTypes.Role, request.Username.Equals("admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Operator")
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return Ok(new
        {
            accessToken = new JwtSecurityTokenHandler().WriteToken(token),
            expiresIn = 28800,
            user = new
            {
                request.Username,
                role = request.Username.Equals("admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Operator"
            }
        });
    }

    private static bool IsValidUser(LoginRequest request)
    {
        return (request.Username.Equals("admin", StringComparison.OrdinalIgnoreCase) && request.Password == "OmsAdmin123!") ||
               (request.Username.Equals("operator", StringComparison.OrdinalIgnoreCase) && request.Password == "OmsOperator123!");
    }
}
