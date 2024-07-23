using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace toreligo.Configurations;

public class JwtConfig
{
    public static string Secret { get; set; } = "<>";
    public static TimeSpan ExpireTime = TimeSpan.FromSeconds(150);

    public static readonly TokenValidationParameters TokenValidateParameters = new()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Secret)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        RequireExpirationTime = false,
        ClockSkew = TimeSpan.Zero
    };

    public static readonly TokenValidationParameters TokenRefreshValidateParameters = new()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Secret)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false,
        RequireExpirationTime = false,
        ClockSkew = TimeSpan.Zero
    };
}