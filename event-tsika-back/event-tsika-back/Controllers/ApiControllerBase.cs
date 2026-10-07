using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace event_tsika_back.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
