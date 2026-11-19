using System.Text.Json.Nodes;
using AssociationService.Models;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AssociationService.Swagger;

public class ExemplesSwagger : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema cible)
        {
            return;
        }

        if (context.Type == typeof(DemandeAssociation))
        {
            cible.Description = "Le statut et la date envoyés sont ignorés. La création force En attente et ajoute le créateur comme président.";
            cible.Example = JsonNode.Parse(
                """
                {
                  "nom": "Club Photo",
                  "description": "Sorties photo sur le campus",
                  "categorie": 1,
                  "faculte": 1,
                  "codePermanentCreateur": "REND20261106"
                }
                """);
        }
    }
}

public class DescriptionsSwagger : IOperationFilter
{
    private static readonly Dictionary<string, string> Resumes = new()
    {
        ["Test"] = "Vérifie que le contrôleur répond.",
        ["ObtenirAssociations"] = "Liste les associations.",
        ["ObtenirAssociationParId"] = "Lit une association.",
        ["CreerAssociation"] = "Crée une association.",
        ["ModifierAssociation"] = "Met à jour une association.",
        ["SupprimerAssociation"] = "Supprime une association et ses éléments liés.",
        ["ObtenirMembres"] = "Liste les membres.",
        ["DemanderAdhesion"] = "Dépose une demande d'adhésion.",
        ["AjouterMembre"] = "Accepte ou refuse une demande d'adhésion.",
        ["ModifierRoleMembre"] = "Change le rôle d'un membre.",
        ["RetirerMembre"] = "Retire un membre.",
        ["ObtenirEvenements"] = "Liste les événements.",
        ["CreerEvenement"] = "Crée un événement planifié.",
        ["ModifierEvenement"] = "Met à jour un événement qui n'a pas commencé.",
        ["SupprimerEvenement"] = "Supprime un événement qui n'a pas commencé.",
        ["ObtenirDemandesAdhesion"] = "Liste les demandes en attente."
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (Resumes.TryGetValue(context.MethodInfo.Name, out var resume))
        {
            operation.Summary = resume;
        }
    }
}
