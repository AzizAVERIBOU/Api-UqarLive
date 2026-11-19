using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace AssociationService.Tests;

[Collection(AssoCollection.Name)]
public class ContratAssoTests
{
    private readonly AssoFixture _fixture;

    public ContratAssoTests(AssoFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Sante_Retourne200()
    {
        using var client = _fixture.Factory.CreateClient();
        var reponse = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
    }

    [Fact]
    public async Task CreerAssociation_CodeInconnu_Retourne400()
    {
        using var client = _fixture.Factory.CreateClient();
        var reponse = await client.PostAsJsonAsync("/api/Association/CreerAssociation", Corps("INCONNU"));
        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task CreerAssociation_AuthIndisponible_Retourne500()
    {
        using var client = _fixture.Factory.CreateClient();
        _fixture.Auth.Indisponible = true;
        try
        {
            var reponse = await client.PostAsJsonAsync("/api/Association/CreerAssociation", Corps(AssoFixture.CodeActif));
            Assert.Equal(HttpStatusCode.InternalServerError, reponse.StatusCode);
        }
        finally
        {
            _fixture.Auth.Indisponible = false;
        }
    }

    [Fact]
    public async Task CreerAssociation_ForceLeStatutEnAttenteEtLePresident()
    {
        using var client = _fixture.Factory.CreateClient();
        var cree = await Creer(client);

        Assert.Equal(2, cree.GetProperty("statut").GetInt32());
        var president = cree.GetProperty("membres")[0];
        Assert.Equal(0, president.GetProperty("role").GetInt32());
        Assert.Equal(AssoFixture.CodeActif, president.GetProperty("codePermanent").GetString());
    }

    [Fact]
    public async Task CreerAssociation_NomDejaPris_Retourne400()
    {
        using var client = _fixture.Factory.CreateClient();
        var nom = NomUnique();
        await Creer(client, nom);

        var reponse = await client.PostAsJsonAsync(
            "/api/Association/CreerAssociation",
            Corps(AssoFixture.CodeActif, nom.ToUpperInvariant()));
        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task ObtenirAssociations_ContientLAssociationCreee()
    {
        using var client = _fixture.Factory.CreateClient();
        var nom = NomUnique();
        await Creer(client, nom);

        var reponseHttp = await client.GetAsync("/api/Association/ObtenirAssociations");
        var texte = await reponseHttp.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, reponseHttp.StatusCode);
        using var document = JsonDocument.Parse(texte);
        var noms = document.RootElement.GetProperty("associations").EnumerateArray()
            .Select(association => association.GetProperty("nom").GetString());
        Assert.Contains(nom, noms);
    }

    [Fact]
    public async Task DemanderAdhesion_CreeUneDemandeEnAttente()
    {
        using var client = _fixture.Factory.CreateClient();
        var id = (await Creer(client)).GetProperty("id").GetInt32();

        var reponse = await client.PostAsJsonAsync($"/api/Association/DemanderAdhesion/{id}", new
        {
            codePermanent = AssoFixture.CodeAdherent,
            motivation = "Je veux participer aux sorties",
            role = 4
        });
        var corps = await reponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        Assert.Equal(0, corps.GetProperty("demande").GetProperty("statut").GetInt32());
    }

    [Fact]
    public async Task CreerEvenement_ForceLeStatutPlanifie()
    {
        using var client = _fixture.Factory.CreateClient();
        var id = (await Creer(client)).GetProperty("id").GetInt32();

        var reponse = await client.PostAsJsonAsync(
            $"/api/Association/CreerEvenement/{id}?codePermanentUtilisateur={AssoFixture.CodeActif}",
            new
            {
                titre = "Assemblée",
                description = "Première rencontre",
                dateDebut = "2027-06-01T18:00:00",
                dateFin = "2027-06-01T20:00:00",
                lieu = new
                {
                    numeroCivique = "300",
                    rue = "allée des Ursulines",
                    ville = "Rimouski",
                    province = "QC",
                    codePostal = "G5L 3A1"
                },
                capaciteMax = 40
            });
        var corps = await reponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
        Assert.Equal(0, corps.GetProperty("evenement").GetProperty("statut").GetInt32());
        Assert.Equal("Assemblée", corps.GetProperty("evenement").GetProperty("titre").GetString());
    }

    [Fact]
    public async Task Swagger_PublieLexemple()
    {
        using var client = _fixture.Factory.CreateClient();
        var json = await client.GetStringAsync("/swagger/v1/swagger.json");
        Assert.Contains("Club Photo", json);
        Assert.Contains("Crée une association", json);
    }

    private async Task<JsonElement> Creer(HttpClient client, string? nom = null)
    {
        var reponse = await client.PostAsJsonAsync("/api/Association/CreerAssociation", Corps(AssoFixture.CodeActif, nom));
        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
        return await reponse.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static object Corps(string code, string? nom = null) => new
    {
        nom = nom ?? NomUnique(),
        description = "Club étudiant",
        categorie = 1,
        faculte = 1,
        codePermanentCreateur = code
    };

    private static string NomUnique() => "Club " + Guid.NewGuid().ToString("N")[..8];
}
