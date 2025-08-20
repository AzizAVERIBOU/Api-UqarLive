using System.Text.Json;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Gateway.Services
{
    public class SwaggerAggregatorService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SwaggerAggregatorService> _logger;

        public SwaggerAggregatorService(IHttpClientFactory httpClientFactory, ILogger<SwaggerAggregatorService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<OpenApiDocument> GetAggregatedSwaggerAsync()
        {
            var aggregatedDoc = new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Title = "UQAR Live - API Gateway",
                    Version = "1.0",
                    Description = "API Gateway agrégé pour tous les services"
                },
                Paths = new OpenApiPaths(),
                Components = new OpenApiComponents
                {
                    Schemas = new Dictionary<string, OpenApiSchema>()
                }
            };

            var services = new[]
            {
                new { Name = "Authentification", Url = "https://uqarliveauth-czhfbpapdfcxhuht.canadacentral-01.azurewebsites.net/swagger/v1/swagger.json" },
                new { Name = "Association", Url = "https://uqarliveassociation-hhfucadyb0dtaugm.canadacentral-01.azurewebsites.net/swagger/v1/swagger.json" },
                new { Name = "Market", Url = "https://uqarlivemarket-cpapfjgzbhdtesf8.canadacentral-01.azurewebsites.net/swagger/v1/swagger.json" },
                new { Name = "Bibliotheque", Url = "https://uqarlivebibliotheque-emg2a5dtbchha9hr.canadacentral-01.azurewebsites.net/swagger/v1/swagger.json" },
                new { Name = "Cafetaria", Url = "https://uqarlivecafetaria-gqbyd6h4f2faesfv.canadacentral-01.azurewebsites.net/swagger/v1/swagger.json" }
            };

            foreach (var service in services)
            {
                try
                {
                    var httpClient = _httpClientFactory.CreateClient();
                    var response = await httpClient.GetAsync(service.Url);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var swaggerJson = await response.Content.ReadAsStringAsync();
                        var reader = new OpenApiStringReader();
                        var serviceDoc = reader.Read(swaggerJson, out var diagnostic);

                        if (serviceDoc != null)
                        {
                            // Ajouter les chemins avec préfixe
                            foreach (var path in serviceDoc.Paths)
                            {
                                var newPath = $"/{service.Name.ToLower()}{path.Key}";
                                aggregatedDoc.Paths[newPath] = path.Value;
                            }

                            // Ajouter les schémas
                            if (serviceDoc.Components?.Schemas != null)
                            {
                                foreach (var schema in serviceDoc.Components.Schemas)
                                {
                                    var schemaKey = $"{service.Name}_{schema.Key}";
                                    aggregatedDoc.Components.Schemas[schemaKey] = schema.Value;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Impossible de récupérer le Swagger pour {ServiceName}", service.Name);
                }
            }

            return aggregatedDoc;
        }
    }
} 