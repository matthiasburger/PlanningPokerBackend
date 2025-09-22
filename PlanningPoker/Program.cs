using PlanningPoker.Hubs;
using PlanningPoker.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// CORS for your frontend(s)
string corsPolicy = "ppokercors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        string[] origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? new string[0];
        policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddSignalR();

// DI services
builder.Services.AddSingleton<RoomManager>();

// Background cleanup
IConfigurationSection cleanupCfg = builder.Configuration.GetSection("RoomCleanup");
if (cleanupCfg.GetValue("Enabled", true))
{
    builder.Services.AddHostedService<RoomCleanupService>();
}

WebApplication app = builder.Build();

app.UseRouting();
app.UseCors(corsPolicy);

app.MapGet("/health", () => Results.Ok(new { ok = true }));

// Map hub
app.MapHub<PokerHub>("/hub");

app.Run();