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
builder.Services.AddSingleton<DocumentationPasserelle>();

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
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/passerelle.json", "UQAR Live");
    options.DocumentTitle = "UQAR Live";
    options.DefaultModelsExpandDepth(2);
});

app.MapGet("/", () => Results.Ok(new
{
    message = "UQAR Live",
    sante = "/health",
    documentation = "/swagger",
    authentification = "/auth"
}));
app.MapGet("/health", () => Results.Ok("Healthy"));
app.MapGet("/swagger/passerelle.json", async (DocumentationPasserelle documentation, CancellationToken annulation) =>
    Results.Content(await documentation.ObtenirAsync(annulation), "application/json"));

app.MapWhen(
    contexte => !contexte.Request.Path.StartsWithSegments("/swagger")
                && contexte.Request.Path != "/"
                && contexte.Request.Path != "/health",
    branche => branche.UseOcelot().Wait());

app.Run();

public partial class Program
{
}
