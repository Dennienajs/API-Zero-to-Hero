namespace Movies.Api.Auth;

public static class IdentityExtensions
{
    public const string UserIdClaimType = "userid";
    
    public static Guid? GetUserId(this HttpContext context)
    {
        return context.User.Claims.SingleOrDefault(c => c.Type == UserIdClaimType)?.Value is { } id 
            && Guid.TryParse(id, out var userId)
                ? userId
                : null;
    }
}