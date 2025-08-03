using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [ApiController]
    [Route("cafetaria")]
    [ApiExplorerSettings(GroupName = "cafetaria")]
    public class CafetariaController : ControllerBase
    {
        // Ce contrôleur sert uniquement pour Swagger
        // Les vraies requêtes sont routées par Ocelot
        
        /// <summary>
        /// Obtenir tous les éléments du menu
        /// </summary>
        [HttpGet("menuitems")]
        public IActionResult GetMenuItems()
        {
            return Ok("Proxy - Cafetaria Service");
        }

        /// <summary>
        /// Créer un nouvel élément de menu
        /// </summary>
        [HttpPost("menuitems")]
        public IActionResult CreateMenuItem()
        {
            return Ok("Proxy - Cafetaria Service");
        }

        /// <summary>
        /// Obtenir le menu du jour
        /// </summary>
        [HttpGet("menu")]
        public IActionResult GetMenu()
        {
            return Ok("Proxy - Cafetaria Service");
        }

        /// <summary>
        /// Créer un menu du jour
        /// </summary>
        [HttpPost("menu")]
        public IActionResult CreateMenu()
        {
            return Ok("Proxy - Cafetaria Service");
        }

        /// <summary>
        /// Obtenir les horaires
        /// </summary>
        [HttpGet("horaires")]
        public IActionResult GetHoraires()
        {
            return Ok("Proxy - Cafetaria Service");
        }

        /// <summary>
        /// Créer un horaire
        /// </summary>
        [HttpPost("horaires")]
        public IActionResult CreateHoraire()
        {
            return Ok("Proxy - Cafetaria Service");
        }
    }
} 