using Microsoft.AspNetCore.Mvc;
using AssociationService.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using AssociationService.Models.Enumerations; 

namespace AssociationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AssociationController : ControllerBase
    {
        private readonly ILogger<AssociationController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public AssociationController(ILogger<AssociationController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "Service d'association opérationnel" });
        }

        // Gestion des associations
        [HttpGet("ObtenirAssociations")]
        public async Task<IActionResult> ObtenirAssociations()
        {
            return Ok(new { message = "Méthode à implémenter" });
        }

        [HttpGet("ObtenirAssociation/{id}")]
        public async Task<IActionResult> ObtenirAssociation(int id)
        {
            return Ok(new { message = "Méthode à implémenter" });
        }

        [HttpPost("CreerAssociation")]
        public async Task<IActionResult> CreerAssociation([FromBody] DemandeAssociation demande)
        {
            return Ok(new { message = "Méthode à implémenter" });
        }

        [HttpPut("ModifierAssociation/{id}")]
        public async Task<IActionResult> ModifierAssociation(int id, [FromBody] Association association)
        {
            return Ok(new { message = "Méthode à implémenter" });
        }

        [HttpDelete("SupprimerAssociation/{id}")]
        public async Task<IActionResult> SupprimerAssociation(int id)
        {
            return Ok(new { message = "Méthode à implémenter" });
        }

        // Gestion des membres
        [HttpGet("ObtenirMembres/{id}")]
        public async Task<IActionResult> ObtenirMembres(int id)
        {
            return Ok(new { message = "Méthode à implémenter" });
        }

        [HttpPost("AjouterMembre/{id}")]
        public async Task<IActionResult> AjouterMembre(int id, [FromBody] DemandeAdhesion demande)
        {
            return Ok(new { message = "Méthode à implémenter" });
        }

        [HttpPut("ModifierRoleMembre/{id}/{codePermanent}")]
        public async Task<IActionResult> ModifierRoleMembre(int id, string codePermanent, [FromBody] RoleMembre nouveauRole)
        {
            return Ok(new { message = "Méthode à implémenter" });
        }

        [HttpDelete("RetirerMembre/{id}/{codePermanent}")]
        public async Task<IActionResult> RetirerMembre(int id, string codePermanent)
        {
            return Ok(new { message = "Méthode à implémenter" });
        }

        // Gestion des événements
        [HttpGet("ObtenirEvenements/{id}")]
        public async Task<IActionResult> ObtenirEvenements(int id)
        {
            return Ok(new { message = "Méthode à implémenter" });
        }

        [HttpPost("CreerEvenement/{id}")]
        public async Task<IActionResult> CreerEvenement(int id, [FromBody] Evenement evenement)
        {
            return Ok(new { message = "Méthode à implémenter" });
        }

        [HttpPut("ModifierEvenement/{id}/{evenementId}")]
        public async Task<IActionResult> ModifierEvenement(int id, int evenementId, [FromBody] Evenement evenement)
        {
            return Ok(new { message = "Méthode à implémenter" });
        }

        [HttpDelete("SupprimerEvenement/{id}/{evenementId}")]
        public async Task<IActionResult> SupprimerEvenement(int id, int evenementId)
        {
            return Ok(new { message = "Méthode à implémenter" });
        }

    
    }
} 