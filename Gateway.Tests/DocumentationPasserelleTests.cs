using System.Net;
using System.Text;
using System.Text.Json;
using Gateway.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gateway.Tests;

public class DocumentationPasserelleTests
{
    [Fact]
    public async Task Obtenir_PublieLesCheminsSousAuth()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Routes:0:UpstreamPathTemplate"] = "/auth/{everything}",
                ["Routes:0:DownstreamPathTemplate"] = "/api/Authentification/{everything}",
                ["Routes:0:DownstreamScheme"] = "https",
                ["Routes:0:DownstreamHostAndPorts:0:Host"] = "api-uqarlive.onrender.com",
                ["Routes:0:DownstreamHostAndPorts:0:Port"] = "443"
            })
            .Build();

        var handler = new ReponseFixe(
            """
            {
              "openapi": "3.0.4",
              "info": { "title": "Auth", "version": "v1" },
              "paths": {
                "/api/Authentification/Connexion": { "post": { "summary": "Connexion" } },
                "/health": { "get": {} }
              },
              "components": { "schemas": { "DemandeConnexion": { "type": "object" } } }
            }
            """);
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var service = new DocumentationPasserelle(new UsineHttp(handler), configuration, loggerFactory.CreateLogger<DocumentationPasserelle>());

        using var document = JsonDocument.Parse(await service.ObtenirAsync(CancellationToken.None));
        var racine = document.RootElement;

        Assert.Equal("https://api-uqarlive.onrender.com/swagger/v1/swagger.json", handler.Adresse?.ToString());
        Assert.Equal("Connexion", racine.GetProperty("paths").GetProperty("/auth/Connexion").GetProperty("post").GetProperty("summary").GetString());
        Assert.False(racine.GetProperty("paths").TryGetProperty("/health", out _));
        Assert.False(racine.GetProperty("paths").TryGetProperty("/api/Authentification/Connexion", out _));
        Assert.Equal("/", racine.GetProperty("servers")[0].GetProperty("url").GetString());
        Assert.True(racine.GetProperty("components").GetProperty("schemas").TryGetProperty("DemandeConnexion", out _));
        Assert.Equal("UQAR Live", racine.GetProperty("info").GetProperty("title").GetString());
    }

    [Fact]
    public async Task Obtenir_PublieLesCheminsSousBiblio()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Routes:0:UpstreamPathTemplate"] = "/biblio/{everything}",
                ["Routes:0:DownstreamPathTemplate"] = "/api/Bibliotheque/{everything}",
                ["Routes:0:DownstreamScheme"] = "https",
                ["Routes:0:DownstreamHostAndPorts:0:Host"] = "uqarlive-bibliotheque.onrender.com",
                ["Routes:0:DownstreamHostAndPorts:0:Port"] = "443"
            })
            .Build();

        var handler = new ReponseFixe(
            """
            {
              "openapi": "3.0.4",
              "info": { "title": "Bibliotheque", "version": "v1" },
              "paths": {
                "/api/Bibliotheque/salles": { "get": { "summary": "Salles" } },
                "/health": { "get": {} }
              }
            }
            """);
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var service = new DocumentationPasserelle(new UsineHttp(handler), configuration, loggerFactory.CreateLogger<DocumentationPasserelle>());

        using var document = JsonDocument.Parse(await service.ObtenirAsync(CancellationToken.None));
        var racine = document.RootElement;

        Assert.Equal("https://uqarlive-bibliotheque.onrender.com/swagger/v1/swagger.json", handler.Adresse?.ToString());
        Assert.Equal("Salles", racine.GetProperty("paths").GetProperty("/biblio/salles").GetProperty("get").GetProperty("summary").GetString());
        Assert.False(racine.GetProperty("paths").TryGetProperty("/health", out _));
        Assert.False(racine.GetProperty("paths").TryGetProperty("/api/Bibliotheque/salles", out _));
        Assert.Contains("/biblio", racine.GetProperty("info").GetProperty("description").GetString());
    }

    private sealed class ReponseFixe : HttpMessageHandler
    {
        private readonly string _corps;
        public Uri? Adresse { get; private set; }

        public ReponseFixe(string corps) => _corps = corps;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Adresse = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_corps, Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class UsineHttp : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public UsineHttp(HttpMessageHandler handler) => _handler = handler;

        public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false);
    }
}
