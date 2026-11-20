using System.Net;
using System.Text.Json.Nodes;

namespace Gateway.Services;

public sealed record EntreeDocumentation(string Cle, string Titre);

public class DocumentationPasserelle
{
    private static readonly Dictionary<string, string> Titres = new(StringComparer.OrdinalIgnoreCase)
    {
        ["/auth"] = "Authentification",
        ["/biblio"] = "Bibliothèque",
        ["/marche"] = "Marché",
        ["/cafete"] = "Cafétéria",
        ["/asso"] = "Association"
    };

    private readonly IHttpClientFactory _clients;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DocumentationPasserelle> _logger;

    public DocumentationPasserelle(
        IHttpClientFactory clients,
        IConfiguration configuration,
        ILogger<DocumentationPasserelle> logger)
    {
        _clients = clients;
        _configuration = configuration;
        _logger = logger;
    }

    public IReadOnlyList<EntreeDocumentation> Entrees()
    {
        var liste = new List<EntreeDocumentation>();
        foreach (var route in _configuration.GetSection("Routes").GetChildren())
        {
            var modele = route["UpstreamPathTemplate"];
            if (string.IsNullOrWhiteSpace(modele))
            {
                continue;
            }

            var prefixe = Prefixe(modele);
            if (prefixe.Length <= 1)
            {
                continue;
            }

            liste.Add(new EntreeDocumentation(prefixe.TrimStart('/'), Titre(prefixe)));
        }

        return liste;
    }

    public async Task<string?> ObtenirAsync(string cle, CancellationToken annulation)
    {
        var route = Trouver(cle);
        if (route == null)
        {
            return null;
        }

        var prefixeAmont = Prefixe(route["UpstreamPathTemplate"]!);
        var titre = Titre(prefixeAmont);
        var modeleAval = route["DownstreamPathTemplate"];
        var hote = route["DownstreamHostAndPorts:0:Host"];
        if (string.IsNullOrWhiteSpace(modeleAval) || string.IsNullOrWhiteSpace(hote))
        {
            return Injoignable(titre, prefixeAmont);
        }

        var adresse = AdresseSwagger(route["DownstreamScheme"], hote, route["DownstreamHostAndPorts:0:Port"]);
        var client = _clients.CreateClient("DocumentationAval");
        for (var essai = 1; essai <= 3; essai++)
        {
            try
            {
                using var reponse = await client.GetAsync(adresse, annulation);
                if (reponse.IsSuccessStatusCode)
                {
                    var json = await reponse.Content.ReadAsStringAsync(annulation);
                    var recu = JsonNode.Parse(json)?.AsObject();
                    if (recu == null)
                    {
                        return Injoignable(titre, prefixeAmont);
                    }

                    var document = Reecrire(recu, Prefixe(modeleAval), prefixeAmont);
                    Completer(document, titre, prefixeAmont, joignable: true);
                    return Serialiser(document);
                }

                _logger.LogWarning("Swagger aval indisponible : {Adresse} {Statut} essai {Essai}", adresse, (int)reponse.StatusCode, essai);
                if (!EstReveil(reponse.StatusCode) || essai == 3)
                {
                    return Injoignable(titre, prefixeAmont);
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
            {
                _logger.LogWarning(ex, "Swagger aval illisible : {Adresse} essai {Essai}", adresse, essai);
                var delaiEpuise = ex is TaskCanceledException && !annulation.IsCancellationRequested;
                if (essai == 3 || (delaiEpuise && essai == 2))
                {
                    return Injoignable(titre, prefixeAmont);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(3), annulation);
        }

        return Injoignable(titre, prefixeAmont);
    }

    internal static JsonObject Reecrire(JsonObject source, string prefixeAval, string prefixeAmont)
    {
        var copie = source.DeepClone().AsObject();
        var nouveaux = new JsonObject();
        if (copie["paths"] is JsonObject chemins)
        {
            foreach (var chemin in chemins)
            {
                if (!chemin.Key.StartsWith(prefixeAval, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var reste = chemin.Key[prefixeAval.Length..];
                nouveaux[prefixeAmont + reste] = chemin.Value?.DeepClone();
            }
        }

        copie["paths"] = nouveaux;
        return copie;
    }

    private IConfigurationSection? Trouver(string cle)
    {
        var cherche = "/" + cle.Trim().Trim('/');
        foreach (var route in _configuration.GetSection("Routes").GetChildren())
        {
            var modele = route["UpstreamPathTemplate"];
            if (string.IsNullOrWhiteSpace(modele))
            {
                continue;
            }

            if (string.Equals(Prefixe(modele), cherche, StringComparison.OrdinalIgnoreCase))
            {
                return route;
            }
        }

        return null;
    }

    private static void Completer(JsonObject document, string titre, string prefixe, bool joignable)
    {
        if (document["info"] is not JsonObject info)
        {
            info = new JsonObject();
            document["info"] = info;
        }

        info["title"] = "UQAR Live — " + titre;
        info["version"] ??= "1.0";
        info["description"] = joignable
            ? "Try it out appelle cette passerelle. Les chemins commencent par " + prefixe + "."
            : "L'instance gratuite se réveille. Rechargez cette page dans un instant. Les chemins commenceront par " + prefixe + ".";
        document["servers"] = new JsonArray(new JsonObject { ["url"] = "/" });
    }

    private static string Injoignable(string titre, string prefixe)
    {
        var document = new JsonObject
        {
            ["openapi"] = "3.0.4",
            ["paths"] = new JsonObject()
        };
        Completer(document, titre, prefixe, joignable: false);
        return Serialiser(document);
    }

    private static string Serialiser(JsonObject document) =>
        document.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

    private static bool EstReveil(HttpStatusCode statut) =>
        statut is HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout;

    private static string Titre(string prefixe) =>
        Titres.TryGetValue(prefixe, out var titre) ? titre : prefixe.TrimStart('/');

    private static string Prefixe(string modele) =>
        modele.Replace("/{everything}", "", StringComparison.Ordinal);

    private static string AdresseSwagger(string? scheme, string hote, string? port)
    {
        var protocole = string.IsNullOrWhiteSpace(scheme) ? "https" : scheme;
        var avecPort = port is "443" or "80" or null or ""
            ? ""
            : $":{port}";
        return $"{protocole}://{hote}{avecPort}/swagger/v1/swagger.json";
    }
}
