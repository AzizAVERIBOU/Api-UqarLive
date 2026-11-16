using System.Text.Json.Nodes;
using MarketService.Models;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MarketService.Swagger;

public class ExemplesSwagger : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema cible)
        {
            return;
        }

        if (context.Type == typeof(Produit))
        {
            cible.Description = "La création enregistre toujours le statut Disponible. Le prix va de 0 à 10 000. categorieId désigne une catégorie existante.";
            cible.Example = JsonNode.Parse(
                """
                {
                  "titre": "Calculatrice",
                  "description": "Scientifique, bon état",
                  "prix": 25,
                  "codePermanentVendeur": "REND20261106",
                  "modePaiementSouhaite": 1,
                  "contactVendeur": "rendu.auth@example.com",
                  "localisation": "Pavillon principal",
                  "categorieId": 1
                }
                """);
        }
        else if (context.Type == typeof(Categorie))
        {
            cible.Description = "Une catégorie inactive n'apparaît plus dans la liste.";
            cible.Example = JsonNode.Parse(
                """
                {
                  "nom": "Fournitures",
                  "description": "Matériel de cours"
                }
                """);
        }
        else if (context.Type == typeof(StatutProduit))
        {
            cible.Description = "1 = Disponible, 2 = Réservé, 3 = Vendu, 4 = Supprimé.";
        }
        else if (context.Type == typeof(ModePaiement))
        {
            cible.Description = "1 = Espèces, 2 = Virement, 3 = PayPal.";
        }
    }
}

public class DescriptionsSwagger : IOperationFilter
{
    private static readonly Dictionary<string, string> Resumes = new()
    {
        ["GetProduits"] = "Liste les annonces, avec filtre de catégorie, de statut ou de vendeur.",
        ["GetProduit"] = "Lit une annonce.",
        ["CreateProduit"] = "Publie une annonce pour un compte actif.",
        ["UpdateProduit"] = "Met à jour le texte, le prix et le contact. Le vendeur et la catégorie restent ceux d'origine.",
        ["UpdateStatutProduit"] = "Change le statut. Vendu enregistre la date de vente.",
        ["DeleteProduit"] = "Retire l'annonce.",
        ["GetCategories"] = "Liste les catégories actives.",
        ["GetCategorie"] = "Lit une catégorie active et ses annonces.",
        ["CreateCategorie"] = "Ajoute une catégorie active.",
        ["UpdateCategorie"] = "Met à jour une catégorie, y compris son activation."
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (Resumes.TryGetValue(context.MethodInfo.Name, out var resume))
        {
            operation.Summary = resume;
        }
    }
}
