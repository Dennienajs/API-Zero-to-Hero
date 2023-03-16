using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;

namespace Movies.Api.Controllers.V2;

[ApiController]
public class ApiControllerBase : ControllerBase
{
    protected Guid? UserId => HttpContext.GetUserId();
}

[Authorize]
public class AuthorizeControllerBase : ApiControllerBase
{
    protected new Guid UserId => base.UserId!.Value;
}
