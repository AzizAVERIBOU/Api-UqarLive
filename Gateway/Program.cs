using Gateway.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Swashbuckle.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddHttpClient("DocumentationAval", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});
builder.Services.AddHttpClient("Reveil", client =>
{
    client.Timeout = TimeSpan.FromSeconds(90);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("UQARLive-Gateway");
});
builder.Services.AddSingleton<DocumentationPasserelle>();
builder.Services.AddSingleton<ReveilServices>();

var environnement = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (!string.Equals(environnement, "Testing", StringComparison.OrdinalIgnoreCase))
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();
var reveil = app.Services.GetRequiredService<ReveilServices>();
app.Use(async (contexte, suivant) =>
{
    if (contexte.Request.Path == "/"
        || contexte.Request.Path == "/health"
        || contexte.Request.Path.StartsWithSegments("/swagger"))
    {
        _ = reveil.Demarrer();
    }

    await suivant();
});
var documentation = app.Services.GetRequiredService<DocumentationPasserelle>();
app.UseSwaggerUI(options =>
{
    foreach (var entree in documentation.Entrees())
    {
        options.SwaggerEndpoint($"/swagger/{entree.Cle}.json", entree.Titre);
    }

    options.DocumentTitle = "UQAR Live";
    options.DefaultModelsExpandDepth(2);
    options.InjectJavascript("/reveil.js");
});

app.MapGet("/", () => Results.Ok(new
{
    message = "UQAR Live",
    sante = "/health",
    documentation = "/swagger",
    authentification = "/auth",
    bibliotheque = "/biblio",
    marche = "/marche",
    cafete = "/cafete",
    association = "/asso"
}));
app.MapGet("/health", () => Results.Ok("Healthy"));
app.MapGet("/reveil.js", () => Results.Text(ReveilServices.ScriptNavigateur, "text/javascript"));
app.MapGet("/swagger/{cle}.json", async (string cle, DocumentationPasserelle documentation, CancellationToken annulation) =>
{
    var json = await documentation.ObtenirAsync(cle, annulation);
    return json == null
        ? Results.NotFound()
        : Results.Content(json, "application/json");
});

// /, /health, /reveil.js et /swagger restent sur la passerelle.
app.MapWhen(
    contexte => !contexte.Request.Path.StartsWithSegments("/swagger")
                && contexte.Request.Path != "/"
                && contexte.Request.Path != "/health"
                && contexte.Request.Path != "/reveil.js",
    branche => branche.UseOcelot().Wait());

app.Run();

public partial class Program
{
}
