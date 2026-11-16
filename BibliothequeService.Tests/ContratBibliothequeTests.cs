using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace BibliothequeService.Tests;

[Collection(BiblioCollection.Name)]
public class ContratBibliothequeTests
{
    private static int _compteur;
    private readonly BiblioFixture _fixture;

    public ContratBibliothequeTests(BiblioFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Sante_Retourne200()
    {
        using var client = _fixture.Factory.CreateClient();
        var reponse = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
    }

    [Fact]
    public async Task CreerSalle_Retourne201EnCamelCase()
    {
        using var client = _fixture.Factory.CreateClient();
        var reponse = await client.PostAsJsonAsync("/api/Bibliotheque/salles", CorpsSalle("A-201"));
        var texte = await reponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
        Assert.Contains("\"nom\"", texte);
        Assert.DoesNotContain("\"Nom\"", texte);
    }

    [Fact]
    public async Task CreerSalle_NomDejaPris_Retourne400()
    {
        using var client = _fixture.Factory.CreateClient();
        var nom = NomUnique();
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/Bibliotheque/salles", CorpsSalle(nom))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/Bibliotheque/salles", CorpsSalle(nom))).StatusCode);
    }

    [Fact]
    public async Task CreerSalle_CapaciteOuTypeInvalide_Retourne400()
    {
        using var client = _fixture.Factory.CreateClient();
        var capacite = await client.PostAsJsonAsync("/api/Bibliotheque/salles", CorpsSalle(NomUnique(), capacite: 0));
        var type = await client.PostAsJsonAsync("/api/Bibliotheque/salles", CorpsSalle(NomUnique(), typeSalle: 0));

        Assert.Equal(HttpStatusCode.BadRequest, capacite.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, type.StatusCode);
    }

    [Fact]
    public async Task CreerReservation_CodeInconnu_Retourne400()
    {
        using var client = _fixture.Factory.CreateClient();
        var reponse = await client.PostAsJsonAsync("/api/Bibliotheque/reservations", CorpsReservation(1, "INCONNU"));
        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task CreerReservation_AuthIndisponible_Retourne500()
    {
        using var client = _fixture.Factory.CreateClient();
        _fixture.Auth.Indisponible = true;
        try
        {
            var reponse = await client.PostAsJsonAsync("/api/Bibliotheque/reservations", CorpsReservation(1, BiblioFixture.CodeActif));
            Assert.Equal(HttpStatusCode.InternalServerError, reponse.StatusCode);
        }
        finally
        {
            _fixture.Auth.Indisponible = false;
        }
    }

    [Fact]
    public async Task CreerReservation_SalleAbsente_Retourne400()
    {
        using var client = _fixture.Factory.CreateClient();
        var reponse = await client.PostAsJsonAsync("/api/Bibliotheque/reservations", CorpsReservation(999_999, BiblioFixture.CodeActif));
        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task CreerReservation_CreneauxQuiSeChevauchent_Retourne400()
    {
        using var client = _fixture.Factory.CreateClient();
        var salleId = await CreerSalle(client);
        var premier = await client.PostAsJsonAsync("/api/Bibliotheque/reservations", CorpsReservation(salleId, BiblioFixture.CodeActif, 5, 9));
        var second = await client.PostAsJsonAsync("/api/Bibliotheque/reservations", CorpsReservation(salleId, BiblioFixture.CodeActif, 7, 11));

        Assert.Equal(HttpStatusCode.Created, premier.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task CreerReservation_CreneauxAdjacents_Retourne201EnAttente()
    {
        using var client = _fixture.Factory.CreateClient();
        var salleId = await CreerSalle(client);
        var premier = await client.PostAsJsonAsync("/api/Bibliotheque/reservations", CorpsReservation(salleId, BiblioFixture.CodeActif, 3, 5));
        var second = await client.PostAsJsonAsync("/api/Bibliotheque/reservations", CorpsReservation(salleId, BiblioFixture.CodeActif, 5, 7));

        Assert.Equal(HttpStatusCode.Created, premier.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        var corps = await second.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, corps.GetProperty("statut").GetInt32());
    }

    [Fact]
    public async Task CreerReservation_DatePassee_Retourne400()
    {
        using var client = _fixture.Factory.CreateClient();
        var salleId = await CreerSalle(client);
        var reponse = await client.PostAsJsonAsync(
            "/api/Bibliotheque/reservations",
            CorpsReservation(salleId, BiblioFixture.CodeActif, date: Hier()));

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task CreerReservation_TropDeParticipants_Retourne400()
    {
        using var client = _fixture.Factory.CreateClient();
        var salleId = await CreerSalle(client, capacite: 8);
        var reponse = await client.PostAsJsonAsync(
            "/api/Bibliotheque/reservations",
            CorpsReservation(salleId, BiblioFixture.CodeActif, participants: 9));

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task DesactiverSalle_AvecReservationEnAttente_Retourne400()
    {
        using var client = _fixture.Factory.CreateClient();
        var nom = NomUnique();
        var creation = await client.PostAsJsonAsync("/api/Bibliotheque/salles", CorpsSalle(nom));
        var salle = await creation.Content.ReadFromJsonAsync<JsonElement>();
        var id = salle.GetProperty("id").GetInt32();
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync(
            "/api/Bibliotheque/reservations",
            CorpsReservation(id, BiblioFixture.CodeActif))).StatusCode);

        var reponse = await client.PutAsJsonAsync($"/api/Bibliotheque/salles/{id}", new
        {
            id,
            nom,
            localisation = "Aile A",
            typeSalle = 1,
            capacite = 8,
            estActive = false
        });

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task Swagger_PublieLesExemples()
    {
        using var client = _fixture.Factory.CreateClient();
        var json = await client.GetStringAsync("/swagger/v1/swagger.json");
        Assert.Contains("A-201", json);
        Assert.Contains("Réserve un créneau", json);
    }

    private async Task<int> CreerSalle(HttpClient client, int capacite = 8)
    {
        var reponse = await client.PostAsJsonAsync("/api/Bibliotheque/salles", CorpsSalle(NomUnique(), capacite: capacite));
        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
        var salle = await reponse.Content.ReadFromJsonAsync<JsonElement>();
        return salle.GetProperty("id").GetInt32();
    }

    private static string NomUnique() => $"S-{Interlocked.Increment(ref _compteur)}";

    private static object CorpsSalle(string nom, int capacite = 8, int typeSalle = 1) => new
    {
        nom,
        localisation = "Aile A",
        typeSalle,
        capacite
    };

    private static object CorpsReservation(
        int salleId,
        string code,
        int heureDebut = 5,
        int heureFin = 9,
        int? participants = 4,
        string? date = null) => new
    {
        salleId,
        codePermanentReservateur = code,
        titreReservation = "Revision",
        dateReservation = date ?? Demain(),
        heureDebut,
        heureFin,
        nombreParticipants = participants
    };

    private static string Demain() => JourQuebec().AddDays(1).ToString("yyyy-MM-dd");

    private static string Hier() => JourQuebec().AddDays(-1).ToString("yyyy-MM-dd");

    private static DateOnly JourQuebec()
    {
        TimeZoneInfo zone;
        try
        {
            zone = TimeZoneInfo.FindSystemTimeZoneById("America/Toronto");
        }
        catch (TimeZoneNotFoundException)
        {
            zone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        }

        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone));
    }
}
