using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using toreligo.Configurations;
using toreligo.Domain.Authentication.Entities;
using toreligo.Domain.Database;

namespace toreligo.Domain.Authentication;

public class AuthService
{
    private readonly EntityStorage entityStorage;

    public AuthService(EntityStorage entityStorage)
    {
        this.entityStorage = entityStorage;
    }

    public bool TryValidateTokens(string jwtToken, string refreshToken, out AuthSession? storedToken)
    {
        storedToken = null;
        try
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var tokenInVerification = jwtTokenHandler.ValidateToken(jwtToken,
                JwtConfig.TokenRefreshValidateParameters,
                out var validatedToken);

            if (validatedToken is JwtSecurityToken jwtSecurityToken)
            {
                var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase);

                if (result == false)
                    return false;
            }

            storedToken = entityStorage.Select<AuthSession>()
                .FirstOrDefault(x => x.RefreshToken == refreshToken);
            if (storedToken == default)
                return false;

            if (storedToken.IsUsed || storedToken.IsRevoked)
                return false;

            var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti);
            if (storedToken.JwtTokenId != jti?.Value)
                return false;

            if (storedToken.ExpiredAt < DateTime.UtcNow)
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public AuthSession CreateSession(UserData userData)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.ASCII.GetBytes(JwtConfig.Secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("Id", userData.Id.ToString()),
                new Claim("Name", userData.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    DateTime.Now.ToUniversalTime().ToString(CultureInfo.InvariantCulture))
            }),
            Expires = DateTime.UtcNow.Add(JwtConfig.ExpireTime),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = jwtTokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = jwtTokenHandler.WriteToken(token);

        var refreshToken = GenerateRefreshToken();
        var authSession = new AuthSession
        {
            UserDataId = userData.Id,
            RefreshToken = refreshToken,
            JwtToken = jwtToken,
            JwtTokenId = token.Id,
            AddedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddDays(14),
            IsUsed = false,
            IsRevoked = false
        };
        return entityStorage.CreateEntity(authSession);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512(passwordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return computedHash.SequenceEqual(passwordHash);
    }
}