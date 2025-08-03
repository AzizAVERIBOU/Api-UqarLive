using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [ApiController]
    [Route("bibliotheque")]
    [ApiExplorerSettings(GroupName = "bibliotheque")]
    public class BibliothequeController : ControllerBase
    {
        // Ce contrôleur sert uniquement pour Swagger
        // Les vraies requêtes sont routées par Ocelot
        
        /// <summary>
        /// Obtenir toutes les salles
        /// </summary>
        [HttpGet("salles")]
        public IActionResult GetSalles()
        {
            return Ok("Proxy - Bibliotheque Service");
        }

        /// <summary>
        /// Obtenir une salle par ID
        /// </summary>
        [HttpGet("salles/{id}")]
        public IActionResult GetSalle(int id)
        {
            return Ok("Proxy - Bibliotheque Service");
        }

        /// <summary>
        /// Créer une nouvelle salle
        /// </summary>
        [HttpPost("salles")]
        public IActionResult CreateSalle()
        {
            return Ok("Proxy - Bibliotheque Service");
        }

        /// <summary>
        /// Obtenir les salles disponibles
        /// </summary>
        [HttpGet("salles/disponibles")]
        public IActionResult GetSallesDisponibles()
        {
            return Ok("Proxy - Bibliotheque Service");
        }

        /// <summary>
        /// Obtenir toutes les réservations
        /// </summary>
        [HttpGet("reservations")]
        public IActionResult GetReservations()
        {
            return Ok("Proxy - Bibliotheque Service");
        }

        /// <summary>
        /// Créer une nouvelle réservation
        /// </summary>
        [HttpPost("reservations")]
        public IActionResult CreateReservation()
        {
            return Ok("Proxy - Bibliotheque Service");
        }

        /// <summary>
        /// Obtenir les heures disponibles
        /// </summary>
        [HttpGet("heures")]
        public IActionResult GetHeures()
        {
            return Ok("Proxy - Bibliotheque Service");
        }
    }
} 