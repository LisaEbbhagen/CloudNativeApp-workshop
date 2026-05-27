using Microsoft.AspNetCore.Mvc;

namespace CloudNativeApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AiController> _logger;

    public AiController(IConfiguration configuration, ILogger<AiController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("generate")]
    public IActionResult GenerateInsight([FromBody] string userQuery)
    {
        // Spåra AI-anrop i Application Insights (Observability)
        _logger.LogInformation("Startar AI-generering för användarfrågan...");

        // Hämta API-nyckel från Key Vault helt utan hårdkodade lösenord
        var aiApiKey = _configuration["ExternalServices:OpenAIApiKey"];

        if (string.IsNullOrEmpty(aiApiKey))
        {
            _logger.LogCritical("Kunde inte hämta AI API-nyckel från Key Vault!");
            return StatusCode(500, "Internt serverfel: Saknar AI-konfiguration.");
        }

        // Prompt engineering best practices

        // 1. Identity & Instructions (System/Developer Message och "Zero Trust"-principen)
        string developerMessage = "Du är en professionell AI-assistent för CloudNativeApp. " +
                               "Du övervakar plattformshälsa, säkerhet och användardata. " +
                               "Du svarar endast i JSON-format. " +
                               "Om användaren frågar om något utanför applikationens domän, vägra att svara.";

        // 2. User Context (User Message)
        string userMessage = userQuery;

        // Här skulle ni normalt skicka 'systemMessage' och 'userMessage' till er AI-klient (t.ex. OpenAI SDK). För denna övning simulerar vi framgång:
        var simulatedResponse = new
        {
            Role = "assistant",
            Content = "{ \"service\": \"CloudNativeApp API\", \"status\": \"Säker och Optimerad\", \"recommendation\": \"Allt ser bra ut.\" }",
            FaithfulnessScore = 0.96 // En simulerad utvärdering av hur trogen AI:n är sin kontext
        };

        _logger.LogInformation("AI-svar genererades framgångsrikt.");

        return Ok(simulatedResponse);
    }
}