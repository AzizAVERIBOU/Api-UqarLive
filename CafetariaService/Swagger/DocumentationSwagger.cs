using System.Text.Json.Nodes;
using CafetariaService.Models;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CafetariaService.Swagger;

public class ExemplesSwagger : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema cible)
        {
            return;
        }

        if (context.Type == typeof(MenuItem))
        {
            cible.Description = "Le prix va de 0 à 1 000.";
            cible.Example = JsonNode.Parse(
                """
                {
                  "nom": "Soupe",
                  "description": "Soupe du jour",
                  "prix": 4.5,
                  "disponible": true
                }
                """);
        }
        else if (context.Type == typeof(DemandeMenu))
        {
            cible.Description = "La date est unique. platIds désigne des plats déjà présents dans le catalogue.";
            cible.Example = JsonNode.Parse(
                """
                {
                  "date": "2026-11-17",
                  "description": "Menu du mardi",
                  "platIds": [1]
                }
                """);
        }
        else if (context.Type == typeof(Horaire))
        {
            cible.Description = "1 = lundi, 5 = vendredi. La fermeture est après l'ouverture.";
            cible.Example = JsonNode.Parse(
                """
                {
                  "jourDeLaSemaine": 1,
                  "heureOuverture": "08:00:00",
                  "heureFermeture": "16:00:00"
                }
                """);
        }
    }
}

public class DescriptionsSwagger : IOperationFilter
{
    private static readonly Dictionary<string, string> Resumes = new()
    {
        ["GetMenuItems"] = "Liste le catalogue.",
        ["AddMenuItem"] = "Ajoute un plat au catalogue.",
        ["UpdateMenuItem"] = "Met à jour un plat.",
        ["DeleteMenuItem"] = "Retire un plat du catalogue.",
        ["GetMenuDuJour"] = "Lit le menu actif du jour, selon le calendrier du Québec.",
        ["CreerMenuDuJour"] = "Crée le menu d'une date à partir de plats du catalogue.",
        ["GetHorairesSemaine"] = "Regroupe les horaires par jour.",
        ["UpdateHorairesSemaine"] = "Remplace tous les horaires de la semaine.",
        ["GetHoraireDuJour"] = "Lit l'horaire d'un jour."
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (Resumes.TryGetValue(context.MethodInfo.Name, out var resume))
        {
            operation.Summary = resume;
        }
    }
}
