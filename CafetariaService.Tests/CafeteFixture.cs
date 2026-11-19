using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace CafetariaService.Tests;

public sealed class CafeteFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _postgres;
    private string? _chainePrecedente;
    private string? _environnementPrecedent;

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

        Factory = new WebApplicationFactory<Program>();
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
}

[CollectionDefinition(Name)]
public sealed class CafeteCollection : ICollectionFixture<CafeteFixture>
{
    public const string Name = "Cafete";
}
