using Microsoft.AspNetCore.Mvc;

namespace CloudNativeApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatusController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public StatusController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }

    [HttpGet("secret")]
    public IActionResult GetSecret()
    {
        var secretValue = _configuration["AppSecret"];

        if (string.IsNullOrEmpty(secretValue))
        {
            return NotFound("Ingen hemlighet hittades.");
        }

        return Ok(new { SecretMessage = secretValue });
    }
}