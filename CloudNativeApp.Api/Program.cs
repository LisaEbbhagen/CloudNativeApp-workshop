using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Aktiverar OpenTelemetry och skickar data automatiskt till Azure Monitor / Application Insights
builder.Services.AddOpenTelemetry().UseAzureMonitor();

builder.Services.AddControllers();

builder.Services.AddOpenApi();

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

app.UseAuthorization();

app.MapControllers();
app.Run();