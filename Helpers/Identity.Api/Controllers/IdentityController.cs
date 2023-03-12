using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Api.Controllers;

[ApiController]
public class IdentityController : ControllerBase
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(12);
    private readonly string _tokenSecret;
    private readonly string _issuer;
    private readonly string _audience;

    public IdentityController(IConfiguration configuration)
    {
        _tokenSecret = configuration["Jwt:Key"] ?? throw new($"Jwt:Key configuration not configured");
        _issuer = configuration["Jwt:Issuer"] ?? throw new($"Jwt:Issuer configuration not configured");
        _audience = configuration["Jwt:Audience"] ?? throw new($"Jwt:Audience configuration not configured");
    }
    
    [HttpPost("token")]
    public IActionResult GenerateToken([FromBody]TokenGenerationRequest request)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, request.Email),
            new(JwtRegisteredClaimNames.Email, request.Email),
            new("userid", request.UserId.ToString())
        };
        
        foreach (var (key, value) in request.CustomClaims)
        {
            JsonElement jsonElement = (JsonElement)value;
            var valueType = jsonElement.ValueKind switch
            {
                JsonValueKind.True => ClaimValueTypes.Boolean,
                JsonValueKind.False => ClaimValueTypes.Boolean,
                JsonValueKind.Number => ClaimValueTypes.Double,
                _ => ClaimValueTypes.String
            };
            
            Claim claim = new(key, value.ToString()!, valueType);
            claims.Add(claim);
        }

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new(claims),
            Expires = DateTime.UtcNow.Add(TokenLifetime),
            Issuer = _issuer,
            IssuedAt = DateTime.UtcNow,
            Audience = _audience,
            SigningCredentials = new(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_tokenSecret)), 
                    SecurityAlgorithms.HmacSha256Signature)
        };

        JwtSecurityTokenHandler tokenHandler = new();
        SecurityToken? token = tokenHandler.CreateToken(tokenDescriptor);

        var jwt = tokenHandler.WriteToken(token);
        return Ok(jwt);
    }
}
