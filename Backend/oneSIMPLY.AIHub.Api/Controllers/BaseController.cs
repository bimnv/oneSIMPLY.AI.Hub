using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace oneSIMPLY.AIHub.Api.Controllers;

public abstract class BaseController : ControllerBase
{
    protected int? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim != null && int.TryParse(claim.Value, out int userId))
            return userId;
        return null;
    }

    protected string? GetUserEmail() => User.FindFirst(ClaimTypes.Email)?.Value;
}
