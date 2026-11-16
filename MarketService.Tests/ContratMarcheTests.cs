using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace MarketService.Tests;

[Collection(MarcheCollection.Name)]
public class ContratMarcheTests
{
    private readonly MarcheFixture _fixture;

    public ContratMarcheTests(MarcheFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Sante_Retourne200()
    {
        using var client = _fixture.Factory.CreateClient();
        var reponse = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
    }

    [Fact]
    public async Task CreerCategorie_Retourne201EnCamelCase()
    {
        using var client = _fixture.Factory.CreateClient();
        var reponse = await client.PostAsJsonAsync("/api/Market/categories", new { nom = "Fournitures" });
        var texte = await reponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
        Assert.Contains("\"nom\"", texte);
        Assert.DoesNotContain("\"Nom\"", texte);
    }

    [Fact]
    public async Task CreerProduit_CodeInconnu_Retourne400()
    {
        using var client = _fixture.Factory.CreateClient();
        var categorieId = await CreerCategorie(client);
        var reponse = await client.PostAsJsonAsync("/api/Market/produits", CorpsProduit(categorieId, "INCONNU"));
        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task CreerProduit_AuthIndisponible_Retourne500()
    {
        using var client = _fixture.Factory.CreateClient();
        var categorieId = await CreerCategorie(client);
        _fixture.Auth.Indisponible = true;
        try
        {
            var reponse = await client.PostAsJsonAsync("/api/Market/produits", CorpsProduit(categorieId, MarcheFixture.CodeActif));
            Assert.Equal(HttpStatusCode.InternalServerError, reponse.StatusCode);
        }
        finally
        {
            _fixture.Auth.Indisponible = false;
        }
    }

    [Fact]
    public async Task CreerProduit_CategorieAbsente_Retourne400()
    {
        using var client = _fixture.Factory.CreateClient();
        var reponse = await client.PostAsJsonAsync("/api/Market/produits", CorpsProduit(999_999, MarcheFixture.CodeActif));
        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task CreerProduit_ForceLeStatutDisponible()
    {
        using var client = _fixture.Factory.CreateClient();
        var categorieId = await CreerCategorie(client);
        var reponse = await client.PostAsJsonAsync("/api/Market/produits", CorpsProduit(categorieId, MarcheFixture.CodeActif, statut: 4));
        var corps = await reponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
        Assert.Equal(1, corps.GetProperty("statut").GetInt32());
        Assert.False(corps.TryGetProperty("categorie", out _));
    }

    [Fact]
    public async Task ModifierProduit_NeChangePasLeVendeur()
    {
        using var client = _fixture.Factory.CreateClient();
        var categorieId = await CreerCategorie(client);
        var creation = await client.PostAsJsonAsync("/api/Market/produits", CorpsProduit(categorieId, MarcheFixture.CodeActif));
        var cree = await creation.Content.ReadFromJsonAsync<JsonElement>();
        var id = cree.GetProperty("id").GetInt32();

        var modification = await client.PutAsJsonAsync($"/api/Market/produits/{id}", new
        {
            titre = "Calculatrice neuve",
            prix = 30,
            codePermanentVendeur = "AUTRE",
            categorieId = 999_999,
            modePaiementSouhaite = 2
        });
        Assert.Equal(HttpStatusCode.NoContent, modification.StatusCode);

        var lu = await client.GetFromJsonAsync<JsonElement>($"/api/Market/produits/{id}");
        Assert.Equal("Calculatrice neuve", lu.GetProperty("titre").GetString());
        Assert.Equal(MarcheFixture.CodeActif, lu.GetProperty("codePermanentVendeur").GetString());
        Assert.Equal(categorieId, lu.GetProperty("categorieId").GetInt32());
    }

    [Fact]
    public async Task CreerProduit_PrixHorsBorne_Retourne400()
    {
        using var client = _fixture.Factory.CreateClient();
        var categorieId = await CreerCategorie(client);
        var reponse = await client.PostAsJsonAsync("/api/Market/produits", CorpsProduit(categorieId, MarcheFixture.CodeActif, prix: 10001));
        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task Swagger_PublieLesExemples()
    {
        using var client = _fixture.Factory.CreateClient();
        var json = await client.GetStringAsync("/swagger/v1/swagger.json");
        Assert.Contains("Calculatrice", json);
        Assert.Contains("Publie une annonce", json);
    }

    private async Task<int> CreerCategorie(HttpClient client)
    {
        var reponse = await client.PostAsJsonAsync("/api/Market/categories", new { nom = "Fournitures" });
        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
        var categorie = await reponse.Content.ReadFromJsonAsync<JsonElement>();
        return categorie.GetProperty("id").GetInt32();
    }

    private static object CorpsProduit(int categorieId, string code, int statut = 1, decimal prix = 25) => new
    {
        titre = "Calculatrice",
        prix,
        codePermanentVendeur = code,
        categorieId,
        modePaiementSouhaite = 1,
        statut
    };
}
