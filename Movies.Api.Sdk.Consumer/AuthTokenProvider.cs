using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Movies.Api.Sdk.Consumer;

public class AuthTokenProvider
{
    private string _cachedToken = string.Empty;
    private static readonly SemaphoreSlim Lock = new(1, 1);
    
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthTokenProvider> _logger;


    public AuthTokenProvider(HttpClient httpClient, ILogger<AuthTokenProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GetTokenAsync()
    {
        _logger.LogInformation($"{nameof(AuthTokenProvider)} - {nameof(GetTokenAsync)}");
        
        if (!string.IsNullOrWhiteSpace(_cachedToken))
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(_cachedToken);
            var expiryTimeText = jwt.Claims.Single(c => c.Type == "exp").Value;
            var expiryTime = UnixTimeStampToDateTime(double.Parse(expiryTimeText));

            if (expiryTime > DateTime.UtcNow)
            {
                _logger.LogInformation("Using cached auth token");
                return _cachedToken;
            }
        }
        
        await Lock.WaitAsync();
        _cachedToken = await RefreshAuthToken();
        Lock.Release();
        

        return _cachedToken;
    }

    private async Task<string> RefreshAuthToken()
    {
        _logger.LogInformation("Refreshing auth token...");
        
        var response = await _httpClient.PostAsJsonAsync("https://localhost:5003/token", new
        {
            userId = "45a57255-29c6-43d8-b17a-4e65698bd182",
            email = "admin@dch.com",
            customClaims = new Dictionary<string, object> 
            {
                { "admin", true },
                { "trusted_member", true },
            }
        });
        
        var token = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("Auth token refreshed!");

        return token;
    }
    
    private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
            .AddSeconds(unixTimeStamp)
            .ToLocalTime();
    }
}
