using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Movies.Api.Auth;

public class AdminAuthRequirement : IAuthorizationHandler, IAuthorizationRequirement
{
    private readonly string _apiKey;

    public AdminAuthRequirement(string apiKey)
    {
        _apiKey = apiKey;
    }
    
    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        if(context.User.HasClaim(AuthConstants.AdminUserClaimName, "true"))
        {
            context.Succeed(this);
            return;
        }

        if (context.Resource is not HttpContext { } httpContext)
        {
            return;
        }
        
        if(!httpContext.Request.Headers.TryGetValue(AuthConstants.ApiKeyHeaderName, out var apiKey))
        {
            context.Fail();
            return;
        }
        
        if(apiKey != _apiKey)
        {
            context.Fail();
            return;
        }


        var identity = (ClaimsIdentity)httpContext.User.Identity!;
        
        var userId = (await GetUserIdFromApiKey()).ToString()!;
        
        identity.AddClaim(new(
            IdentityExtensions.UserIdClaimType, 
            userId));
        
        context.Succeed(this);
    }

    private async Task<Guid> GetUserIdFromApiKey()
    {
        // TODO: Get the user id belonging to the api key... For now we'll just return a dummy id
        Guid id = Guid.Parse("F1343F32-6161-409E-AF74-FBC8CA8958CC");
        return await Task.FromResult(id);
    }
}
