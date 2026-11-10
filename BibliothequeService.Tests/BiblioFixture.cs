using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Testcontainers.PostgreSql;

namespace BibliothequeService.Tests;

public sealed class BiblioFixture : IAsyncLifetime
{
    public const string CodeActif = "REND20261106";

    private PostgreSqlContainer? _postgres;
    private string? _chainePrecedente;
    private string? _environnementPrecedent;

    public AuthEtat Auth { get; } = new();
    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder("postgres:18")
            .WithDatabase("uqarlive")
            .Build();
        await _postgres.StartAsync();

        _chainePrecedente = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        _environnementPrecedent = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _postgres.GetConnectionString());
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

        Factory = new BiblioApplication(Auth);
        using var client = Factory.CreateClient();
        var sante = await client.GetAsync("/health");
        sante.EnsureSuccessStatusCode();
    }

    public async Task DisposeAsync()
    {
        Factory?.Dispose();
        if (_postgres != null)
        {
            await _postgres.DisposeAsync();
        }

        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _chainePrecedente);
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", _environnementPrecedent);
    }

    private sealed class BiblioApplication : WebApplicationFactory<Program>
    {
        private readonly AuthEtat _auth;

        public BiblioApplication(AuthEtat auth) => _auth = auth;

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.Configure<HttpClientFactoryOptions>("AuthService", options =>
                {
                    options.HttpMessageHandlerBuilderActions.Add(handler =>
                        handler.PrimaryHandler = new AuthSimulee(_auth));
                });
            });
        }
    }
}

public sealed class AuthEtat
{
    public bool Indisponible { get; set; }
}

public sealed class AuthSimulee : HttpMessageHandler
{
    private readonly AuthEtat _etat;

    public AuthSimulee(AuthEtat etat) => _etat = etat;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_etat.Indisponible)
        {
            throw new HttpRequestException("Auth indisponible");
        }

        var code = request.RequestUri?.AbsolutePath.Split('/').LastOrDefault();
        var statut = code == BiblioFixture.CodeActif ? HttpStatusCode.OK : HttpStatusCode.NotFound;
        return Task.FromResult(new HttpResponseMessage(statut));
    }
}

[CollectionDefinition(Name)]
public sealed class BiblioCollection : ICollectionFixture<BiblioFixture>
{
    public const string Name = "Bibliotheque";
}
