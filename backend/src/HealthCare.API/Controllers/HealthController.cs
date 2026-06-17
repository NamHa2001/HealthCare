using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.API.Controllers;

[AllowAnonymous]
public class HealthController : BaseController
{
    [HttpGet]
    public IActionResult Check() => Ok(new
    {
        status = "healthy",
        version = "1.0.0",
        timestamp = DateTime.UtcNow,
    });
}
