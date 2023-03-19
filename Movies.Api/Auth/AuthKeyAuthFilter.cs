using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Movies.Api.Auth;

public class AuthKeyAuthFilter : IAuthorizationFilter
{
    private const string ApiKeyConfigurationName = "ApiKey";
    private readonly string _apiKey;

    public AuthKeyAuthFilter(IConfiguration configuration)
    {
        _apiKey = configuration[ApiKeyConfigurationName] ?? throw new ArgumentNullException(nameof(configuration), $"{ApiKeyConfigurationName} not found in configuration");
    }
    
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if(!context.HttpContext.Request.Headers.TryGetValue(AuthConstants.ApiKeyHeaderName, out var apiKey))
        {
            context.Result = new UnauthorizedObjectResult("API Key missing");
            return;
        }
        
        if(apiKey != _apiKey)
        {
            context.Result = new UnauthorizedObjectResult("Invalid Api Key");
        }
    }
}
