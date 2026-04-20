using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using FirstMvcApp.Models;

namespace FirstMvcApp.Services;

public class JwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;

    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Generate JWT token for user
    /// </summary>
    public string GenerateToken(User user)
    {
        try
        {
            var jwtSecret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret not configured");
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "FirstMvcApp";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "FirstMvcAppUsers";
            var jwtExpireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim("userId", user.Id)
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtExpireMinutes),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            _logger.LogInformation($"JWT token generated for user {user.Email}");
            return tokenString;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error generating JWT token: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Validate JWT token
    /// </summary>
    public ClaimsPrincipal ValidateToken(string token)
    {
        try
        {
            var jwtSecret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error validating JWT token: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get user ID from token
    /// </summary>
    public string GetUserIdFromToken(string token)
    {
        try
        {
            var principal = ValidateToken(token);
            return principal?.FindFirst("userId")?.Value;
        }
        catch
        {
            return null;
        }
    }
}
