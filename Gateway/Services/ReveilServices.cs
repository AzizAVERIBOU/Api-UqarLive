namespace Gateway.Services;

public class ReveilServices
{
    private readonly IHttpClientFactory _clients;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReveilServices> _logger;
    private readonly TimeSpan _ecart;
    private readonly object _porte = new();
    private DateTime _dernier = DateTime.MinValue;

    public ReveilServices(
        IHttpClientFactory clients,
        IConfiguration configuration,
        ILogger<ReveilServices> logger,
        TimeSpan? ecart = null)
    {
        _clients = clients;
        _configuration = configuration;
        _logger = logger;
        _ecart = ecart ?? TimeSpan.FromSeconds(45);
    }

    public string ScriptNavigateur()
    {
        var adresses = string.Join(", ", AdressesSante().Select(adresse => "\"" + adresse + "\""));
        return "[" + adresses + "].forEach(function (adresse) { fetch(adresse, { mode: \"no-cors\" }).catch(function () {}); });";
    }

    public Task Demarrer()
    {
        lock (_porte)
        {
            var maintenant = DateTime.UtcNow;
            if (maintenant - _dernier < _ecart)
            {
                return Task.CompletedTask;
            }

            _dernier = maintenant;
        }

        return ReveillerAsync();
    }

    private async Task ReveillerAsync()
    {
        var adresses = AdressesSante();
        if (adresses.Count == 0)
        {
            return;
        }

        var client = _clients.CreateClient("Reveil");
        await Task.WhenAll(adresses.Select(adresse => AppelerAsync(client, adresse)));
    }

    private async Task AppelerAsync(HttpClient client, string adresse)
    {
        try
        {
            using var reponse = await client.GetAsync(adresse);
            _logger.LogInformation("Réveil {Adresse} {Statut}", adresse, (int)reponse.StatusCode);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "Réveil inabouti {Adresse}", adresse);
        }
    }

    private IReadOnlyList<string> AdressesSante()
    {
        var adresses = new List<string>();
        foreach (var route in _configuration.GetSection("Routes").GetChildren())
        {
            var hote = route["DownstreamHostAndPorts:0:Host"];
            if (string.IsNullOrWhiteSpace(hote))
            {
                continue;
            }

            var scheme = route["DownstreamScheme"];
            var protocole = string.IsNullOrWhiteSpace(scheme) ? "https" : scheme;
            var port = route["DownstreamHostAndPorts:0:Port"];
            var avecPort = port is "443" or "80" or null or "" ? "" : $":{port}";
            adresses.Add($"{protocole}://{hote}{avecPort}/health");
        }

        return adresses;
    }
}
