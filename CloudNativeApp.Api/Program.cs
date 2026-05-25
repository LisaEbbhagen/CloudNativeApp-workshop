using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var appInsightsConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    // Kör bara den här raden om det faktiskt finns en anslutningssträng konfigurerad
    // Aktiverar OpenTelemetry och skickar data automatiskt till Azure Monitor / Application Insights
    builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
    {
        options.ConnectionString = appInsightsConnectionString;
    });
}

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("StrictSecurityPolicy", policyBuilder =>
    {
        //    policyBuilder
        //    .WithOrigins("null");

        //});
        options.AddPolicy("StrictSecurityPolicy", policyBuilder =>
        {
            policyBuilder
                .WithOrigins("https://min-sakra-frondend-app.azurewebsites.net") // Ersätt med din faktiska tillåtna ursprung
                .WithMethods("GET", "POST") // Endast tillåtna HTTP-metoder, principen om att begränsa till det som behövs
                .AllowAnyHeader(); //alternativt för ännu striktare .WithHeaders("Content-Type", "Authorization") för att specificera vilka headers som är tillåtna
        });
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
    app.UseCors("StrictSecurityPolicy");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.Run();