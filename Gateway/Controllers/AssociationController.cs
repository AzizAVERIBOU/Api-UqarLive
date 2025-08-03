using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "association")]
    public class AssociationController : ControllerBase
    {
        // Ce contrôleur sert uniquement pour Swagger
        // Les vraies requêtes sont routées par Ocelot
        
        /// <summary>
        /// Obtenir toutes les associations
        /// </summary>
        [HttpGet]
        public IActionResult GetAssociations()
        {
            return Ok("Proxy - Association Service");
        }

        /// <summary>
        /// Obtenir une association par ID
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetAssociation(int id)
        {
            return Ok("Proxy - Association Service");
        }

        /// <summary>
        /// Créer une nouvelle association
        /// </summary>
        [HttpPost]
        public IActionResult CreateAssociation()
        {
            return Ok("Proxy - Association Service");
        }

        /// <summary>
        /// Mettre à jour une association
        /// </summary>
        [HttpPut("{id}")]
        public IActionResult UpdateAssociation(int id)
        {
            return Ok("Proxy - Association Service");
        }

        /// <summary>
        /// Supprimer une association
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult DeleteAssociation(int id)
        {
            return Ok("Proxy - Association Service");
        }

        /// <summary>
        /// Gérer les demandes d'adhésion
        /// </summary>
        [HttpPost("demandes-adhesion")]
        public IActionResult DemandeAdhesion()
        {
            return Ok("Proxy - Association Service");
        }

        /// <summary>
        /// Gérer les événements
        /// </summary>
        [HttpPost("evenements")]
        public IActionResult CreateEvenement()
        {
            return Ok("Proxy - Association Service");
        }
    }
} 