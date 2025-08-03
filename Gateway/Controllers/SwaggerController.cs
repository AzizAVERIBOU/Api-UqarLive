using Microsoft.AspNetCore.Mvc;
using Gateway.Services;
using System.Text.Json;

namespace Gateway.Controllers
{
    [ApiController]
    [Route("swagger")]
    public class SwaggerController : ControllerBase
    {
        private readonly SwaggerAggregatorService _swaggerAggregator;

        public SwaggerController(SwaggerAggregatorService swaggerAggregator)
        {
            _swaggerAggregator = swaggerAggregator;
        }

        /// <summary>
        /// Obtenir le Swagger agrégé en JSON
        /// </summary>
        [HttpGet("aggregated/swagger.json")]
        public async Task<IActionResult> GetAggregatedSwagger()
        {
            try
            {
                var aggregatedDoc = await _swaggerAggregator.GetAggregatedSwaggerAsync();
                
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(aggregatedDoc, options);
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erreur lors de l'agrégation Swagger", details = ex.Message });
            }
        }
    }
} 