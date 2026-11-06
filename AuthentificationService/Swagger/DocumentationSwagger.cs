using System.Text.Json.Nodes;
using AuthentificationService.Models;
using AuthentificationService.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using RessourcesPartagees.Enumerations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AuthentificationService.Swagger;

public class ExemplesSwagger : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema cible)
        {
            return;
        }

        if (context.Type == typeof(DemandeInscription))
        {
            cible.Description = "L'inscription publique enregistre toujours le rôle Etudiant, même si une autre valeur est envoyée. Le mot de passe compte au moins 8 caractères. L'adresse et le téléphone sont obligatoires.";
            cible.Example = JsonNode.Parse(
                """
                {
                  "codePermanent": "REND20261106",
                  "email": "rendu.auth@example.com",
                  "motDePasse": "Portfolio1",
                  "prenom": "Rendu",
                  "nom": "Auth",
                  "programme": "Informatique",
                  "faculte": "Sciences",
                  "anneeDebut": 2024,
                  "anneeFin": 2028,
                  "telephone": "4185550199",
                  "role": 0,
                  "adresse": {
                    "numeroCivique": "300",
                    "rue": "Allee des Ursulines",
                    "ville": "Rimouski",
                    "province": "QC",
                    "codePostal": "G5L3A1",
                    "codePermanentUtilisateur": "REND20261106"
                  }
                }
                """);
        }
        else if (context.Type == typeof(DemandeConnexion))
        {
            cible.Description = "Le code permanent et le mot de passe choisis à l'inscription.";
            cible.Example = JsonNode.Parse(
                """
                {
                  "codePermanent": "REND20261106",
                  "motDePasse": "Portfolio1"
                }
                """);
        }
        else if (context.Type == typeof(DemandeChangementMotDePasse))
        {
            cible.Description = "Le nouveau mot de passe compte au moins 8 caractères et doit différer de l'ancien. La confirmation doit être identique.";
            cible.Example = JsonNode.Parse(
                """
                {
                  "ancienMotDePasse": "Portfolio1",
                  "nouveauMotDePasse": "NouveauMot1",
                  "confirmationMotDePasse": "NouveauMot1"
                }
                """);
        }
        else if (context.Type == typeof(RoleUtilisateur))
        {
            cible.Description = "0 Etudiant, 1 Personnel, 2 Admin. L'inscription publique enregistre toujours Etudiant.";
        }
        else if (context.Type == typeof(ReponseToken))
        {
            cible.Description = "Jeton Bearer valable 60 minutes. Le rôle dans le jeton est le texte Etudiant, Personnel ou Admin.";
            cible.Example = JsonNode.Parse(
                """
                {
                  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.exemple",
                  "dateExpiration": "2026-11-06T17:00:00Z",
                  "type": "Bearer",
                  "message": "Connexion réussie",
                  "utilisateur": {
                    "codePermanent": "REND20261106",
                    "email": "rendu.auth@example.com",
                    "nom": "Auth",
                    "prenom": "Rendu",
                    "role": "Etudiant"
                  }
                }
                """);
        }
    }
}

public class DescriptionsSwagger : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var nom = context.MethodInfo.Name;
        if (Resumes.TryGetValue(nom, out var resume))
        {
            operation.Summary = resume;
        }

        if (Details.TryGetValue(nom, out var detail))
        {
            operation.Description = detail;
        }

        var autorise = context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
            || context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() == true;
        if (!autorise || context.Document == null)
        {
            return;
        }

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
        });
    }

    private static readonly Dictionary<string, string> Resumes = new()
    {
        ["Test"] = "Vérifie que l'API répond.",
        ["Inscription"] = "Crée un compte étudiant et renvoie un jeton.",
        ["Connexion"] = "Vérifie le mot de passe et renvoie un jeton.",
        ["RafraichirToken"] = "Renvoie un nouveau jeton à partir du jeton courant.",
        ["Deconnexion"] = "Confirme que le client doit oublier le jeton.",
        ["ObtenirProfil"] = "Lit le profil du compte porté par le jeton.",
        ["ModifierProfil"] = "Met à jour le profil du compte porté par le jeton.",
        ["ChangerMotDePasse"] = "Remplace le mot de passe du compte porté par le jeton.",
        ["VerifierUtilisateur"] = "Dit si un code permanent correspond à un compte actif."
    };

    private static readonly Dictionary<string, string> Details = new()
    {
        ["Inscription"] = "Aucun jeton n'est demandé. Le rôle envoyé est ignoré : le compte devient Etudiant et EstActif passe à vrai. Un mot de passe de moins de 8 caractères, un téléphone vide ou une adresse incomplète donne 400.",
        ["Connexion"] = "Aucun jeton n'est demandé. Un mauvais mot de passe donne 401. Un compte désactivé reste désactivé et donne 401.",
        ["Deconnexion"] = "Le jeton est exigé. Aucune écriture en base : le compte reste actif. Le client doit oublier le jeton.",
        ["VerifierUtilisateur"] = "200 si le code existe et que le compte est actif. 404 sinon. Le rôle est renvoyé sous forme de nombre : 0 Etudiant, 1 Personnel, 2 Admin."
    };
}
