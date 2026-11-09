using System.Text.Json.Nodes;

namespace Gateway.Services;

public class DocumentationPasserelle
{
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

    public async Task<string> ObtenirAsync(CancellationToken annulation)
    {
        JsonObject? document = null;

        foreach (var route in _configuration.GetSection("Routes").GetChildren())
        {
            var modeleAmont = route["UpstreamPathTemplate"];
            var modeleAval = route["DownstreamPathTemplate"];
            var hote = route["DownstreamHostAndPorts:0:Host"];
            if (string.IsNullOrWhiteSpace(modeleAmont)
                || string.IsNullOrWhiteSpace(modeleAval)
                || string.IsNullOrWhiteSpace(hote))
            {
                continue;
            }

            var adresse = AdresseSwagger(route["DownstreamScheme"], hote, route["DownstreamHostAndPorts:0:Port"]);
            try
            {
                var client = _clients.CreateClient("DocumentationAval");
                using var reponse = await client.GetAsync(adresse, annulation);
                if (!reponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Swagger aval indisponible : {Adresse} {Statut}", adresse, (int)reponse.StatusCode);
                    continue;
                }

                var json = await reponse.Content.ReadAsStringAsync(annulation);
                var recu = JsonNode.Parse(json)?.AsObject();
                if (recu == null)
                {
                    continue;
                }

                var reecrit = Reecrire(
                    recu,
                    Prefixe(modeleAval),
                    Prefixe(modeleAmont));
                document = Fusionner(document, reecrit);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
            {
                _logger.LogWarning(ex, "Swagger aval illisible : {Adresse}", adresse);
            }
        }

        document ??= new JsonObject
        {
            ["openapi"] = "3.0.4",
            ["paths"] = new JsonObject()
        };

        if (document["info"] is not JsonObject info)
        {
            info = new JsonObject();
            document["info"] = info;
        }

        info["title"] = "UQAR Live";
        info["version"] = "1.0";
        info["description"] =
            "Documentation de la passerelle. Try it out appelle cette adresse : /auth rejoint l'authentification, /biblio rejoint la bibliothèque. Un service injoignable est ignoré.";
        document["servers"] = new JsonArray(new JsonObject { ["url"] = "/" });

        return document.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
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

    private static JsonObject Fusionner(JsonObject? document, JsonObject ajout)
    {
        if (document == null)
        {
            return ajout;
        }

        if (ajout["paths"] is JsonObject chemins)
        {
            var destination = document["paths"] as JsonObject ?? new JsonObject();
            foreach (var chemin in chemins)
            {
                destination[chemin.Key] = chemin.Value?.DeepClone();
            }

            document["paths"] = destination;
        }

        if (ajout["components"]?["schemas"] is JsonObject schemas)
        {
            var composants = document["components"] as JsonObject ?? new JsonObject();
            var destination = composants["schemas"] as JsonObject ?? new JsonObject();
            foreach (var schema in schemas)
            {
                if (!destination.ContainsKey(schema.Key))
                {
                    destination[schema.Key] = schema.Value?.DeepClone();
                }
            }

            composants["schemas"] = destination;
            document["components"] = composants;
        }

        return document;
    }

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
