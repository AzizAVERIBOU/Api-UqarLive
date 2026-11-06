using AuthentificationService.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace AuthentificationService.Tests;

public sealed class AuthFixture : IAsyncLifetime
{
    public const string CleJwt = "cle-de-test-uniquement-pour-les-tests-automatises";

    private PostgreSqlContainer? _postgres;
    private string? _chainePrecedente;
    private string? _clePrecedente;
    private string? _expirationPrecedente;
    private string? _environnementPrecedent;

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder("postgres:18")
            .WithDatabase("uqarlive")
            .Build();
        await _postgres.StartAsync();

        _chainePrecedente = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        _clePrecedente = Environment.GetEnvironmentVariable("JwtSettings__SecretKey");
        _expirationPrecedente = Environment.GetEnvironmentVariable("JwtSettings__ExpiryInMinutes");
        _environnementPrecedent = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _postgres.GetConnectionString());
        Environment.SetEnvironmentVariable("JwtSettings__SecretKey", CleJwt);
        Environment.SetEnvironmentVariable("JwtSettings__ExpiryInMinutes", "60");
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
        Environment.SetEnvironmentVariable("JwtSettings__SecretKey", _clePrecedente);
        Environment.SetEnvironmentVariable("JwtSettings__ExpiryInMinutes", _expirationPrecedente);
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", _environnementPrecedent);
    }

    public async Task Desactiver(string codePermanent)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthentificationDbContext>();
        var utilisateur = await db.Utilisateurs.SingleAsync(u => u.CodePermanent == codePermanent);
        utilisateur.EstActif = false;
        await db.SaveChangesAsync();
    }

    public async Task<bool> EstActif(string codePermanent)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthentificationDbContext>();
        var utilisateur = await db.Utilisateurs.SingleAsync(u => u.CodePermanent == codePermanent);
        return utilisateur.EstActif;
    }
}

[CollectionDefinition(Name)]
public sealed class AuthCollection : ICollectionFixture<AuthFixture>
{
    public const string Name = "Authentification";
}
