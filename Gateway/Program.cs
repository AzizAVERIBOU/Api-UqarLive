using Microsoft.AspNetCore.HttpOverrides;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Ocelot config
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

// Services divers
builder.Services.AddScoped<Gateway.Services.SwaggerAggregatorService>();
builder.Services.AddHttpClient();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Gateway API",
        Version = "1.0",
        Description = "API Gateway pour notre app console uqar live"
    });
});

// IMPORTANT: écouter sur le port injecté par App Service (WEBSITES_PORT/PORT) ou 5000 par défaut
var port = Environment.GetEnvironmentVariable("PORT") ??
           Environment.GetEnvironmentVariable("WEBSITES_PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Si l'app est derrière un proxy (App Service), activer les forwarded headers
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedHost;
    o.KnownNetworks.Clear();
    o.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

// Swagger toujours dispo
app.UseSwagger();
app.UseSwaggerUI();

// NE PAS forcer la redirection HTTPS sur App Service pour laisser passer /health
// (Tu peux la garder en dev si tu veux, mais pas en prod App Service)
// app.UseHttpsRedirection();

// Ocelot en premier pour le routage des services
await app.UseOcelot();

// Endpoints de base APRÈS Ocelot pour éviter les conflits de routage
// Ces endpoints sont gérés directement par ASP.NET Core, pas par Ocelot
app.MapGet("/health", () => Results.Ok("OK"));
app.MapGet("/", () => Results.Ok("Gateway up"));

app.Run();
