using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Aktiverar OpenTelemetry och skickar data automatiskt till Azure Monitor / Application Insights
builder.Services.AddOpenTelemetry().UseAzureMonitor();

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("StrictSecurityPolicy", policyBuilder =>
    {
        policyBuilder
            .WithOrigins("https://min-sakra-frondend-app.azurewebsites.net") // Ersätt med din faktiska tillåtna ursprung
            .WithMethods("GET", "POST") // Endast tillåtna HTTP-metoder, principen om att begränsa till det som behövs
            .AllowAnyHeader(); //alternativt för ännu striktare .WithHeaders("Content-Type", "Authorization") för att specificera vilka headers som är tillåtna
    });
});

var keyVaultUrl = builder.Configuration["KeyVaultUrl"];

if (!string.IsNullOrEmpty(keyVaultUrl))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl), 
        new DefaultAzureCredential());
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("StrictSecurityPolicy");

app.UseAuthorization();

app.MapControllers();
app.Run();