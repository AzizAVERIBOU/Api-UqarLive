using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace CafetariaService.Tests;

[Collection(CafeteCollection.Name)]
public class ContratCafeteTests
{
    private static int _jour;
    private readonly CafeteFixture _fixture;

    public ContratCafeteTests(CafeteFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Sante_Retourne200()
    {
        using var client = _fixture.Factory.CreateClient();
        var reponse = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
    }

    [Fact]
    public async Task CreerPlat_Retourne201EnCamelCase()
    {
        using var client = _fixture.Factory.CreateClient();
        var reponse = await client.PostAsJsonAsync("/api/Cafetaria/menuitems", new { nom = "Soupe", prix = 4.5 });
        var texte = await reponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
        Assert.Contains("\"nom\"", texte);
        Assert.DoesNotContain("\"Nom\"", texte);
    }

    [Fact]
    public async Task CreerPlat_PrixHorsBorne_Retourne400()
    {
        using var client = _fixture.Factory.CreateClient();
        var reponse = await client.PostAsJsonAsync("/api/Cafetaria/menuitems", new { nom = "Soupe", prix = 1001 });
        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task CreerMenu_PlatInconnu_Retourne400()
    {
        using var client = _fixture.Factory.CreateClient();
        var reponse = await client.PostAsJsonAsync("/api/Cafetaria/menu", new
        {
            date = DateUnique(),
            platIds = new[] { 999_999 }
        });
        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task CreerMenu_LieLePlatDuCatalogue()
    {
        using var client = _fixture.Factory.CreateClient();
        var platId = await CreerPlat(client);
        var reponse = await client.PostAsJsonAsync("/api/Cafetaria/menu", new
        {
            date = DateUnique(),
            platIds = new[] { platId }
        });
        var corps = await reponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
        Assert.Equal("Soupe", corps.GetProperty("platsDuJour")[0].GetProperty("nom").GetString());
    }

    [Fact]
    public async Task CreerMenu_DateDejaPrise_Retourne400()
    {
        using var client = _fixture.Factory.CreateClient();
        var date = DateUnique();
        var platId = await CreerPlat(client);
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/Cafetaria/menu", new { date, platIds = new[] { platId } })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/Cafetaria/menu", new { date, platIds = new[] { platId } })).StatusCode);
    }

    [Fact]
    public async Task RemplacerHoraires_JourEnDouble_Retourne400()
    {
        using var client = _fixture.Factory.CreateClient();
        var reponse = await client.PutAsJsonAsync("/api/Cafetaria/horaires", new[]
        {
            Horaire(1, "08:00:00", "12:00:00"),
            Horaire(1, "13:00:00", "16:00:00")
        });
        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task RemplacerHoraires_FermetureAvantOuverture_Retourne400()
    {
        using var client = _fixture.Factory.CreateClient();
        var reponse = await client.PutAsJsonAsync("/api/Cafetaria/horaires", new[]
        {
            Horaire(1, "16:00:00", "08:00:00")
        });
        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task MenuDuJour_RetourneLeMenuDuQuebec()
    {
        using var client = _fixture.Factory.CreateClient();
        var platId = await CreerPlat(client);
        var creation = await client.PostAsJsonAsync("/api/Cafetaria/menu", new
        {
            date = AujourdhuiQuebec(),
            platIds = new[] { platId }
        });
        Assert.Equal(HttpStatusCode.Created, creation.StatusCode);

        var reponse = await client.GetAsync("/api/Cafetaria/menu/aujourdhui");
        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
    }

    [Fact]
    public async Task Swagger_PublieLesExemples()
    {
        using var client = _fixture.Factory.CreateClient();
        var json = await client.GetStringAsync("/swagger/v1/swagger.json");
        Assert.Contains("Soupe", json);
        Assert.Contains("Crée le menu", json);
    }

    private async Task<int> CreerPlat(HttpClient client)
    {
        var reponse = await client.PostAsJsonAsync("/api/Cafetaria/menuitems", new { nom = "Soupe", prix = 4.5 });
        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
        var plat = await reponse.Content.ReadFromJsonAsync<JsonElement>();
        return plat.GetProperty("id").GetInt32();
    }

    private static string DateUnique() => new DateOnly(2027, 1, 1).AddDays(Interlocked.Increment(ref _jour)).ToString("yyyy-MM-dd");

    private static object Horaire(int jour, string ouverture, string fermeture) => new
    {
        jourDeLaSemaine = jour,
        heureOuverture = ouverture,
        heureFermeture = fermeture
    };

    private static string AujourdhuiQuebec()
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

        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone)).ToString("yyyy-MM-dd");
    }
}
