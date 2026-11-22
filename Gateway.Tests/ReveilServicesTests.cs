using System.Net;
using System.Text;
using Gateway.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gateway.Tests;

public class ReveilServicesTests
{
    [Fact]
    public async Task Demarrer_AppelleLaSanteDeChaqueServiceUneFois()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Routes:0:DownstreamScheme"] = "https",
                ["Routes:0:DownstreamHostAndPorts:0:Host"] = "api-uqarlive.onrender.com",
                ["Routes:0:DownstreamHostAndPorts:0:Port"] = "443",
                ["Routes:1:DownstreamScheme"] = "https",
                ["Routes:1:DownstreamHostAndPorts:0:Host"] = "uqarlive-bibliotheque.onrender.com",
                ["Routes:1:DownstreamHostAndPorts:0:Port"] = "443"
            })
            .Build();

        var handler = new Journal();
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var service = new ReveilServices(new Usine(handler), configuration, loggerFactory.CreateLogger<ReveilServices>());

        await service.Demarrer();
        await service.Demarrer();

        Assert.Equal(2, handler.Adresses.Count);
        Assert.Contains("https://api-uqarlive.onrender.com/health", handler.Adresses);
        Assert.Contains("https://uqarlive-bibliotheque.onrender.com/health", handler.Adresses);
        Assert.Contains("/biblio/salles", ReveilServices.ScriptNavigateur);
        Assert.Contains("/asso/ObtenirAssociations", ReveilServices.ScriptNavigateur);
    }

    private sealed class Journal : HttpMessageHandler
    {
        private readonly object _porte = new();
        public List<string> Adresses { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            lock (_porte)
            {
                Adresses.Add(request.RequestUri!.ToString());
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Healthy", Encoding.UTF8, "text/plain")
            });
        }
    }

    private sealed class Usine : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public Usine(HttpMessageHandler handler) => _handler = handler;

        public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false);
    }
}
