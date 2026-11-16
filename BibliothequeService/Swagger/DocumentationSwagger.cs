using System.Text.Json.Nodes;
using BibliothequeService.Models;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BibliothequeService.Swagger;

public class ExemplesSwagger : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema cible)
        {
            return;
        }

        if (context.Type == typeof(Salle))
        {
            cible.Description = "Le nom est unique. Le type va de 1 à 8 et la capacité est au moins 1.";
            cible.Example = JsonNode.Parse(
                """
                {
                  "nom": "A-201",
                  "localisation": "Aile A",
                  "typeSalle": 1,
                  "capacite": 8,
                  "description": "Salle silencieuse",
                  "estDisponible": true,
                  "equipements": "Tableau, prises"
                }
                """);
        }
        else if (context.Type == typeof(Reservation))
        {
            cible.Description = "Les heures vont de 1 (8 h 00) à 29 (22 h 00). La création enregistre toujours le statut En attente.";
            cible.Example = JsonNode.Parse(
                """
                {
                  "salleId": 1,
                  "codePermanentReservateur": "REND20261106",
                  "titreReservation": "Revision",
                  "dateReservation": "2026-11-10",
                  "heureDebut": 5,
                  "heureFin": 9,
                  "nombreParticipants": 4
                }
                """);
        }
        else if (context.Type == typeof(HeureReservation))
        {
            cible.Description = "1 = 8 h 00, puis un pas de 30 minutes, jusqu'à 29 = 22 h 00.";
        }
    }
}

public class DescriptionsSwagger : IOperationFilter
{
    private static readonly Dictionary<string, string> Resumes = new()
    {
        ["GetSalles"] = "Liste les salles actives.",
        ["GetSalle"] = "Lit une salle et ses réservations.",
        ["CreateSalle"] = "Ajoute une salle au catalogue.",
        ["UpdateSalle"] = "Met à jour une salle. Un nom déjà pris est refusé.",
        ["DeleteSalle"] = "Désactive une salle sans réservation en attente, confirmée ou en cours.",
        ["GetReservations"] = "Liste les réservations.",
        ["GetReservation"] = "Lit une réservation.",
        ["CreateReservation"] = "Réserve un créneau pour un compte actif.",
        ["UpdateReservation"] = "Modifie un créneau et revérifie les conflits.",
        ["DeleteReservation"] = "Annule une réservation.",
        ["GetSallesDisponibles"] = "Filtre les salles libres sur un créneau.",
        ["GetHeuresDisponibles"] = "Liste les créneaux de 8 h 00 à 22 h 00."
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (Resumes.TryGetValue(context.MethodInfo.Name, out var resume))
        {
            operation.Summary = resume;
        }
    }
}
