using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class HomeController : ControllerBase
    {
        /// <summary>
        /// Page d'accueil du Gateway
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(new
            {
                message = "UQAR Live - API Gateway",
                version = "1.0",
                services = new[]
                {
                    "Authentification Service",
                    "Association Service", 
                    "Market Service",
                    "Bibliotheque Service",
                    "Cafetaria Service"
                },
                documentation = "/swagger"
            });
        }

        /// <summary>
        /// Informations sur le Gateway
        /// </summary>
        [HttpGet("info")]
        public IActionResult Info()
        {
            return Ok(new
            {
                name = "UQAR Live API Gateway",
                description = "Point d'entrée unique pour tous les services",
                baseUrl = "http://localhost:5000",
                swagger = "http://localhost:5000/swagger"
            });
        }
    }
} 