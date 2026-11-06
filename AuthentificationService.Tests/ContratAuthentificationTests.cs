using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace AuthentificationService.Tests;

[Collection(AuthCollection.Name)]
public class ContratAuthentificationTests
{
    private const string Racine = "/api/Authentification";
    private readonly HttpClient _client;
    private readonly AuthFixture _fixture;

    public ContratAuthentificationTests(AuthFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task Swagger_MontreLeContrat()
    {
        var reponse = await _client.GetAsync("/swagger/v1/swagger.json");
        var document = await Lire(reponse);

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        Assert.Contains("REND20261106", document.ToString());
        Assert.Contains("0 Etudiant", document.ToString());

        var inscription = document.GetProperty("paths").GetProperty("/api/Authentification/Inscription").GetProperty("post");
        Assert.Equal("Crée un compte étudiant et renvoie un jeton.", inscription.GetProperty("summary").GetString());
        Assert.False(inscription.TryGetProperty("security", out _));

        var deconnexion = document.GetProperty("paths").GetProperty("/api/Authentification/Deconnexion").GetProperty("post");
        Assert.True(deconnexion.TryGetProperty("security", out _));
        Assert.True(document.GetProperty("paths").TryGetProperty("/health", out _));
    }

    [Fact]
    public async Task Sante_Repond()
    {
        var racine = await _client.GetAsync("/");
        var sante = await _client.GetAsync("/health");
        var test = await _client.GetAsync($"{Racine}/test");

        Assert.Equal(HttpStatusCode.OK, racine.StatusCode);
        Assert.Equal("Auth service up", (await Lire(racine)).GetString());
        Assert.Equal(HttpStatusCode.OK, sante.StatusCode);
        Assert.Equal("Healthy", (await Lire(sante)).GetString());
        Assert.Equal(HttpStatusCode.OK, test.StatusCode);
    }

    [Fact]
    public async Task Inscription_RefuseUnMotDePasseTropCourt()
    {
        var reponse = await Inscrire(Code(), "court");

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
        Assert.Equal(
            "Le mot de passe doit contenir au moins 8 caractères.",
            (await Lire(reponse)).GetProperty("message").GetString());
    }

    [Fact]
    public async Task Inscription_RefuseUnTelephoneVide()
    {
        var code = Code();
        var reponse = await _client.PostAsJsonAsync($"{Racine}/Inscription", Corps(code, $"{code}@example.com", telephone: " "));

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task Inscription_RefuseUneAdresseIncomplete()
    {
        var code = Code();
        var corps = Corps(code, $"{code}@example.com");
        corps["adresse"] = new Dictionary<string, string?>
        {
            ["numeroCivique"] = "300",
            ["rue"] = " ",
            ["ville"] = "Rimouski",
            ["province"] = "QC",
            ["codePostal"] = "G5L3A1",
            ["codePermanentUtilisateur"] = code
        };

        var reponse = await _client.PostAsJsonAsync($"{Racine}/Inscription", corps);

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task Inscription_ForceLeRoleEtudiantEtActiveLeCompte()
    {
        var code = Code();
        var reponse = await Inscrire(code);
        var corps = await Lire(reponse);

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        Assert.Equal("Inscription réussie", corps.GetProperty("message").GetString());
        Assert.Equal("Etudiant", corps.GetProperty("utilisateur").GetProperty("role").GetString());
        Assert.False(corps.ToString().Contains("StackTrace", StringComparison.OrdinalIgnoreCase));
        Assert.True(await _fixture.EstActif(code));

        var doublon = await Inscrire(code);
        Assert.Equal(HttpStatusCode.BadRequest, doublon.StatusCode);
        Assert.Equal(
            "Un utilisateur avec cet email ou ce code permanent existe déjà.",
            (await Lire(doublon)).GetProperty("message").GetString());
    }

    [Fact]
    public async Task Connexion_DelivreUnJetonValableSoixanteMinutes()
    {
        var code = Code();
        Assert.Equal(HttpStatusCode.OK, (await Inscrire(code)).StatusCode);

        var mauvais = await Connecter(code, "mauvaisMot");
        Assert.Equal(HttpStatusCode.Unauthorized, mauvais.StatusCode);
        Assert.Equal(
            "Code permanent ou mot de passe incorrect",
            (await Lire(mauvais)).GetProperty("message").GetString());

        var reponse = await Connecter(code, "Portfolio1");
        var corps = await Lire(reponse);
        var expiration = corps.GetProperty("dateExpiration").GetDateTime();

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        Assert.Equal("Connexion réussie", corps.GetProperty("message").GetString());
        Assert.Equal("Etudiant", corps.GetProperty("utilisateur").GetProperty("role").GetString());
        Assert.InRange((expiration - DateTime.UtcNow).TotalMinutes, 58, 62);
    }

    [Fact]
    public async Task CompteDesactive_ResteRefuseEtInvisible()
    {
        var code = Code();
        Assert.Equal(HttpStatusCode.OK, (await Inscrire(code)).StatusCode);
        await _fixture.Desactiver(code);

        var connexion = await Connecter(code, "Portfolio1");
        Assert.Equal(HttpStatusCode.Unauthorized, connexion.StatusCode);
        Assert.Equal("Ce compte est désactivé", (await Lire(connexion)).GetProperty("message").GetString());
        Assert.False(await _fixture.EstActif(code));

        var verification = await _client.GetAsync($"{Racine}/VerifierUtilisateur/{code}");
        var corps = await Lire(verification);
        Assert.Equal(HttpStatusCode.NotFound, verification.StatusCode);
        Assert.False(corps.GetProperty("existe").GetBoolean());
    }

    [Fact]
    public async Task ProfilEtVerification_ExigentUnCompteActif()
    {
        var code = Code();
        var inscrit = await Lire(await Inscrire(code));
        var jeton = inscrit.GetProperty("token").GetString();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jeton);

        var sansJeton = await _fixture.Factory.CreateClient().GetAsync($"{Racine}/ObtenirProfilUtilisateur");
        Assert.Equal(HttpStatusCode.Unauthorized, sansJeton.StatusCode);

        var profil = await Lire(await _client.GetAsync($"{Racine}/ObtenirProfilUtilisateur"));
        Assert.Equal(code, profil.GetProperty("codePermanent").GetString());
        Assert.Equal("4185550199", profil.GetProperty("telephone").GetString());
        Assert.Equal(0, profil.GetProperty("role").GetInt32());
        Assert.True(profil.GetProperty("estActif").GetBoolean());
        Assert.Equal("300", profil.GetProperty("adresse").GetProperty("numeroCivique").GetString());

        var verification = await Lire(await _client.GetAsync($"{Racine}/VerifierUtilisateur/{code}"));
        Assert.True(verification.GetProperty("existe").GetBoolean());
        Assert.Equal(0, verification.GetProperty("role").GetInt32());

        var inconnu = await _client.GetAsync($"{Racine}/VerifierUtilisateur/INCONNU00000");
        Assert.Equal(HttpStatusCode.NotFound, inconnu.StatusCode);
    }

    [Fact]
    public async Task Deconnexion_NEteintPasLeCompte()
    {
        var code = Code();
        var inscrit = await Lire(await Inscrire(code));
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", inscrit.GetProperty("token").GetString());

        var sansJeton = await _fixture.Factory.CreateClient().PostAsync($"{Racine}/Deconnexion", null);
        Assert.Equal(HttpStatusCode.Unauthorized, sansJeton.StatusCode);

        var deconnexion = await Lire(await _client.PostAsync($"{Racine}/Deconnexion", null));
        Assert.Equal("Déconnexion réussie. Le jeton doit être oublié par le client.", deconnexion.GetProperty("message").GetString());
        Assert.True(await _fixture.EstActif(code));

        var verification = await _client.GetAsync($"{Racine}/VerifierUtilisateur/{code}");
        Assert.Equal(HttpStatusCode.OK, verification.StatusCode);
    }

    [Fact]
    public async Task ChangementDeMotDePasse_RemplaceLAncienSecret()
    {
        var code = Code();
        var inscrit = await Lire(await Inscrire(code));
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", inscrit.GetProperty("token").GetString());

        var tropCourt = await Changer("Portfolio1", "court", "court");
        Assert.Equal(HttpStatusCode.BadRequest, tropCourt.StatusCode);
        Assert.Equal(
            "Le mot de passe doit contenir au moins 8 caractères.",
            (await Lire(tropCourt)).GetProperty("message").GetString());

        var different = await Changer("Portfolio1", "NouveauMot1", "AutreMot1");
        Assert.Equal(HttpStatusCode.BadRequest, different.StatusCode);

        var identique = await Changer("Portfolio1", "Portfolio1", "Portfolio1");
        Assert.Equal(HttpStatusCode.BadRequest, identique.StatusCode);

        var mauvaisAncien = await Changer("pasLeBon", "NouveauMot1", "NouveauMot1");
        Assert.Equal(HttpStatusCode.BadRequest, mauvaisAncien.StatusCode);

        var change = await Changer("Portfolio1", "NouveauMot1", "NouveauMot1");
        Assert.Equal(HttpStatusCode.OK, change.StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await Connecter(code, "Portfolio1")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await Connecter(code, "NouveauMot1")).StatusCode);
    }

    private Task<HttpResponseMessage> Inscrire(string code, string motDePasse = "Portfolio1")
    {
        return _client.PostAsJsonAsync(
            $"{Racine}/Inscription",
            Corps(code, $"{code}@example.com", motDePasse));
    }

    private Task<HttpResponseMessage> Connecter(string code, string motDePasse)
    {
        return _client.PostAsJsonAsync($"{Racine}/Connexion", new
        {
            codePermanent = code,
            motDePasse
        });
    }

    private Task<HttpResponseMessage> Changer(string ancien, string nouveau, string confirmation)
    {
        return _client.PostAsJsonAsync($"{Racine}/ChangerMotDePasse", new
        {
            ancienMotDePasse = ancien,
            nouveauMotDePasse = nouveau,
            confirmationMotDePasse = confirmation
        });
    }

    private static Dictionary<string, object?> Corps(
        string code,
        string email,
        string motDePasse = "Portfolio1",
        string telephone = "4185550199")
    {
        return new Dictionary<string, object?>
        {
            ["codePermanent"] = code,
            ["email"] = email,
            ["motDePasse"] = motDePasse,
            ["prenom"] = "Rendu",
            ["nom"] = "Test",
            ["programme"] = "Informatique",
            ["faculte"] = "Sciences",
            ["anneeDebut"] = 2024,
            ["anneeFin"] = 2028,
            ["telephone"] = telephone,
            ["role"] = 2,
            ["adresse"] = new Dictionary<string, string?>
            {
                ["numeroCivique"] = "300",
                ["rue"] = "Allee des Ursulines",
                ["ville"] = "Rimouski",
                ["province"] = "QC",
                ["codePostal"] = "G5L3A1",
                ["codePermanentUtilisateur"] = code
            }
        };
    }

    private static string Code() => "T" + Guid.NewGuid().ToString("N")[..11];

    private static async Task<JsonElement> Lire(HttpResponseMessage reponse)
    {
        var texte = await reponse.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(texte);
        return document.RootElement.Clone();
    }
}
