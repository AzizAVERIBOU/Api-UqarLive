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

        using var document = Lire(await service.ObtenirAsync("auth", CancellationToken.None));
        var racine = document.RootElement;

        Assert.Equal("https://api-uqarlive.onrender.com/swagger/v1/swagger.json", handler.Adresse?.ToString());
        Assert.Equal("Connexion", racine.GetProperty("paths").GetProperty("/auth/Connexion").GetProperty("post").GetProperty("summary").GetString());
        Assert.False(racine.GetProperty("paths").TryGetProperty("/health", out _));
        Assert.False(racine.GetProperty("paths").TryGetProperty("/api/Authentification/Connexion", out _));
        Assert.Equal("/", racine.GetProperty("servers")[0].GetProperty("url").GetString());
        Assert.True(racine.GetProperty("components").GetProperty("schemas").TryGetProperty("DemandeConnexion", out _));
        Assert.Equal("UQAR Live — Authentification", racine.GetProperty("info").GetProperty("title").GetString());
        Assert.Contains("/auth", racine.GetProperty("info").GetProperty("description").GetString());
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

        using var document = Lire(await service.ObtenirAsync("biblio", CancellationToken.None));
        var racine = document.RootElement;

        Assert.Equal("https://uqarlive-bibliotheque.onrender.com/swagger/v1/swagger.json", handler.Adresse?.ToString());
        Assert.Equal("Salles", racine.GetProperty("paths").GetProperty("/biblio/salles").GetProperty("get").GetProperty("summary").GetString());
        Assert.False(racine.GetProperty("paths").TryGetProperty("/health", out _));
        Assert.False(racine.GetProperty("paths").TryGetProperty("/api/Bibliotheque/salles", out _));
        Assert.Contains("/biblio", racine.GetProperty("info").GetProperty("description").GetString());
    }

    [Fact]
    public async Task Obtenir_PublieLesCheminsSousMarche()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Routes:0:UpstreamPathTemplate"] = "/marche/{everything}",
                ["Routes:0:DownstreamPathTemplate"] = "/api/Market/{everything}",
                ["Routes:0:DownstreamScheme"] = "https",
                ["Routes:0:DownstreamHostAndPorts:0:Host"] = "uqarlive-marche.onrender.com",
                ["Routes:0:DownstreamHostAndPorts:0:Port"] = "443"
            })
            .Build();

        var handler = new ReponseFixe(
            """
            {
              "openapi": "3.0.4",
              "info": { "title": "Marche", "version": "v1" },
              "paths": {
                "/api/Market/produits": { "get": { "summary": "Annonces" } },
                "/health": { "get": {} }
              }
            }
            """);
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var service = new DocumentationPasserelle(new UsineHttp(handler), configuration, loggerFactory.CreateLogger<DocumentationPasserelle>());

        using var document = Lire(await service.ObtenirAsync("marche", CancellationToken.None));
        var racine = document.RootElement;

        Assert.Equal("https://uqarlive-marche.onrender.com/swagger/v1/swagger.json", handler.Adresse?.ToString());
        Assert.Equal("Annonces", racine.GetProperty("paths").GetProperty("/marche/produits").GetProperty("get").GetProperty("summary").GetString());
        Assert.False(racine.GetProperty("paths").TryGetProperty("/health", out _));
        Assert.False(racine.GetProperty("paths").TryGetProperty("/api/Market/produits", out _));
        Assert.Contains("/marche", racine.GetProperty("info").GetProperty("description").GetString());
    }

    [Fact]
    public async Task Obtenir_PublieLesCheminsSousCafete()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Routes:0:UpstreamPathTemplate"] = "/cafete/{everything}",
                ["Routes:0:DownstreamPathTemplate"] = "/api/Cafetaria/{everything}",
                ["Routes:0:DownstreamScheme"] = "https",
                ["Routes:0:DownstreamHostAndPorts:0:Host"] = "uqarlive-cafetaria.onrender.com",
                ["Routes:0:DownstreamHostAndPorts:0:Port"] = "443"
            })
            .Build();

        var handler = new ReponseFixe(
            """
            {
              "openapi": "3.0.4",
              "info": { "title": "Cafete", "version": "v1" },
              "paths": {
                "/api/Cafetaria/menuitems": { "get": { "summary": "Catalogue" } },
                "/health": { "get": {} }
              }
            }
            """);
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var service = new DocumentationPasserelle(new UsineHttp(handler), configuration, loggerFactory.CreateLogger<DocumentationPasserelle>());

        using var document = Lire(await service.ObtenirAsync("cafete", CancellationToken.None));
        var racine = document.RootElement;

        Assert.Equal("https://uqarlive-cafetaria.onrender.com/swagger/v1/swagger.json", handler.Adresse?.ToString());
        Assert.Equal("Catalogue", racine.GetProperty("paths").GetProperty("/cafete/menuitems").GetProperty("get").GetProperty("summary").GetString());
        Assert.False(racine.GetProperty("paths").TryGetProperty("/health", out _));
        Assert.False(racine.GetProperty("paths").TryGetProperty("/api/Cafetaria/menuitems", out _));
        Assert.Contains("/cafete", racine.GetProperty("info").GetProperty("description").GetString());
    }

    [Fact]
    public async Task Obtenir_PublieLesCheminsSousAsso()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Routes:0:UpstreamPathTemplate"] = "/asso/{everything}",
                ["Routes:0:DownstreamPathTemplate"] = "/api/Association/{everything}",
                ["Routes:0:DownstreamScheme"] = "https",
                ["Routes:0:DownstreamHostAndPorts:0:Host"] = "uqarlive-association.onrender.com",
                ["Routes:0:DownstreamHostAndPorts:0:Port"] = "443"
            })
            .Build();

        var handler = new ReponseFixe(
            """
            {
              "openapi": "3.0.4",
              "info": { "title": "Association", "version": "v1" },
              "paths": {
                "/api/Association/ObtenirAssociations": { "get": { "summary": "Associations" } },
                "/health": { "get": {} }
              }
            }
            """);
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var service = new DocumentationPasserelle(new UsineHttp(handler), configuration, loggerFactory.CreateLogger<DocumentationPasserelle>());

        using var document = Lire(await service.ObtenirAsync("asso", CancellationToken.None));
        var racine = document.RootElement;

        Assert.Equal("https://uqarlive-association.onrender.com/swagger/v1/swagger.json", handler.Adresse?.ToString());
        Assert.Equal("Associations", racine.GetProperty("paths").GetProperty("/asso/ObtenirAssociations").GetProperty("get").GetProperty("summary").GetString());
        Assert.False(racine.GetProperty("paths").TryGetProperty("/health", out _));
        Assert.False(racine.GetProperty("paths").TryGetProperty("/api/Association/ObtenirAssociations", out _));
        Assert.Contains("/asso", racine.GetProperty("info").GetProperty("description").GetString());
    }

    [Fact]
    public async Task Obtenir_SepareLesServicesEtGardeLInjoignable()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Routes:0:UpstreamPathTemplate"] = "/auth/{everything}",
                ["Routes:0:DownstreamPathTemplate"] = "/api/Authentification/{everything}",
                ["Routes:0:DownstreamScheme"] = "https",
                ["Routes:0:DownstreamHostAndPorts:0:Host"] = "api-uqarlive.onrender.com",
                ["Routes:0:DownstreamHostAndPorts:0:Port"] = "443",
                ["Routes:1:UpstreamPathTemplate"] = "/biblio/{everything}",
                ["Routes:1:DownstreamPathTemplate"] = "/api/Bibliotheque/{everything}",
                ["Routes:1:DownstreamScheme"] = "https",
                ["Routes:1:DownstreamHostAndPorts:0:Host"] = "uqarlive-bibliotheque.onrender.com",
                ["Routes:1:DownstreamHostAndPorts:0:Port"] = "443"
            })
            .Build();

        var handler = new ReponsesParHote(hote => hote.Contains("bibliotheque", StringComparison.Ordinal)
            ? (HttpStatusCode.ServiceUnavailable, "")
            : (HttpStatusCode.OK, """
            {
              "openapi": "3.0.4",
              "info": { "title": "Auth", "version": "v1" },
              "paths": {
                "/api/Authentification/Connexion": { "post": { "summary": "Connexion" } },
                "/api/Bibliotheque/salles": { "get": { "summary": "Salles" } }
              }
            }
            """));
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var service = new DocumentationPasserelle(new UsineHttp(handler), configuration, loggerFactory.CreateLogger<DocumentationPasserelle>());

        var entrees = service.Entrees();
        Assert.Equal(new[] { "auth", "biblio" }, entrees.Select(entree => entree.Cle));
        Assert.Equal(new[] { "Authentification", "Bibliothèque" }, entrees.Select(entree => entree.Titre));

        using var auth = Lire(await service.ObtenirAsync("auth", CancellationToken.None));
        Assert.True(auth.RootElement.GetProperty("paths").TryGetProperty("/auth/Connexion", out _));
        Assert.False(auth.RootElement.GetProperty("paths").TryGetProperty("/biblio/salles", out _));

        using var biblio = Lire(await service.ObtenirAsync("biblio", CancellationToken.None));
        Assert.Equal("UQAR Live — Bibliothèque", biblio.RootElement.GetProperty("info").GetProperty("title").GetString());
        Assert.Contains("injoignable", biblio.RootElement.GetProperty("info").GetProperty("description").GetString());
        Assert.False(biblio.RootElement.GetProperty("paths").EnumerateObject().Any());

        Assert.Null(await service.ObtenirAsync("inconnu", CancellationToken.None));
    }

    private static JsonDocument Lire(string? json)
    {
        Assert.NotNull(json);
        return JsonDocument.Parse(json);
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

    private sealed class ReponsesParHote : HttpMessageHandler
    {
        private readonly Func<string, (HttpStatusCode Statut, string Corps)> _choisir;

        public ReponsesParHote(Func<string, (HttpStatusCode Statut, string Corps)> choisir) => _choisir = choisir;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var (statut, corps) = _choisir(request.RequestUri?.Host ?? "");
            return Task.FromResult(new HttpResponseMessage(statut)
            {
                Content = new StringContent(corps, Encoding.UTF8, "application/json")
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
