using Microsoft.AspNetCore.Mvc;

namespace CloudNativeApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatusController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StatusController> _logger;

    public StatusController(IConfiguration configuration, ILogger<StatusController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("health")]
    public IActionResult GetHealth([FromQuery] bool simulateCrash = false)
    {
        _logger.LogInformation("Health check requested. The application is running.");
        if (simulateCrash)
        {
            _logger.LogError("A simulated application error was triggered during health check.");
            throw new Exception("Critical error: Simulated crash occurred during health check.");
        }
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }

    [HttpGet("secret")]
    public IActionResult GetSecret()
    {
        _logger.LogInformation("Secret retrieval requested. Attempting to access configuration.");
        var secretValue = _configuration["AppSecret"];

        if (string.IsNullOrEmpty(secretValue))
        {
            _logger.LogWarning("No secret found in configuration.");
            return NotFound("No secret found in configuration.");
        }

        _logger.LogInformation("Secret retrieved successfully.");
        return Ok(new { SecretMessage = secretValue });
    }
}


