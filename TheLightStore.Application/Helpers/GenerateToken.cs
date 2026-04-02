using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TheLightStore.Application.Interfaces.Infrastructures;

namespace TheLightStore.Application.Helpers;

public static class JwtHelper
{
    public static string GenerateAccessToken(
        IConfigurationService configuration,
        string userId,
        string email,
        string? userName,
        string? role = null)
    {
        var claims = new List<Claim>
            {
                new Claim("sub", userId),
                new Claim("jti", Guid.NewGuid().ToString()),
                new Claim("UserId", userId),
                new Claim(ClaimTypes.Email, email ?? string.Empty),
                new Claim("UserName", userName ?? "")
            };
        if (!string.IsNullOrWhiteSpace(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        var identity = new ClaimsIdentity(new GenericIdentity(email ?? string.Empty, "Token"), claims);

        var secretKey = configuration.GetJwtKey() ?? "4335d179-a729-489c-82ec-b5ccd05a10f5";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: configuration.GetJwtIssuer(),
            audience: configuration.GetJwtAudience(),
            claims: identity.Claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
    public static string GenerateRefreshToken(
        IConfigurationService configuration,
        string userId,
        string email)
    {
        var claims = new List<Claim>
            {
                new Claim("sub", userId),
                new Claim("jti", Guid.NewGuid().ToString()),
                new Claim("UserId", userId),
                new Claim(ClaimTypes.Email, email ?? string.Empty),
                new Claim("type", "refresh")
            };

        var refreshSecret = configuration.GetJwtRefreshKey() ?? "e8DsXj00N2D6w47+8YMYKQZMKfiL2poOzDAB9OxHUZw=";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(refreshSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var refreshTokenDescriptor = new JwtSecurityToken(
            issuer: configuration.GetJwtIssuer(),
            audience: configuration.GetJwtAudience(),
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(refreshTokenDescriptor);
    }
    public static ClaimsPrincipal? ValidateRefreshToken(IConfigurationService config, string refreshToken)
    {
        var refreshSecret = config.GetJwtRefreshKey() ?? "e8DsXj00N2D6w47+8YMYKQZMKfiL2poOzDAB9OxHUZw=";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(refreshSecret));
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config.GetJwtIssuer(),
                ValidAudience = config.GetJwtAudience(),
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var type = principal.FindFirst("type")?.Value;
            if (type != "refresh") return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
