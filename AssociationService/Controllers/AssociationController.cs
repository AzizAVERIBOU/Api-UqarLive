using Microsoft.AspNetCore.Mvc;
using AssociationService.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using AssociationService.Models.Enumerations;
using AssociationService.Data;
using Microsoft.EntityFrameworkCore;

namespace AssociationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AssociationController : ControllerBase
    {
        private readonly ILogger<AssociationController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AssociationDbContext _context;

        public AssociationController(
            ILogger<AssociationController> logger, 
            IHttpClientFactory httpClientFactory,
            AssociationDbContext context)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _context = context;
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
            try
            {
                if (demande == null)
                {
                    return BadRequest("La demande ne peut pas être nulle");
                }

                // Validation des champs obligatoires
                if (string.IsNullOrWhiteSpace(demande.Nom))
                {
                    return BadRequest("Le nom de l'association est obligatoire");
                }

                if (string.IsNullOrWhiteSpace(demande.Description))
                {
                    return BadRequest("La description est obligatoire");
                }

                // Vérifier si une association avec le même nom existe déjà
                var associationExistante = await _context.Associations
                    .FirstOrDefaultAsync(a => a.Nom.ToLower() == demande.Nom.ToLower());

                if (associationExistante != null)
                {
                    return BadRequest("Une association avec ce nom existe déjà");
                }

                // Vérifier si l'utilisateur existe et est connecté dans le service d'authentification
                var client = _httpClientFactory.CreateClient("AuthService");
                var response = await client.GetAsync($"api/Authentification/VerifierUtilisateur/{demande.CodePermanentCreateur}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("[CreerAssociation] Utilisateur non autorisé : {CodePermanent}, Status: {Status}, Response: {Response}", 
                        demande.CodePermanentCreateur, response.StatusCode, errorContent);
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return BadRequest("L'utilisateur n'existe pas dans le système");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        return BadRequest("L'utilisateur doit être connecté pour créer une association");
                    }
                    else
                    {
                        return BadRequest("L'utilisateur n'est pas autorisé à créer une association");
                    }
                }

                // Créer une nouvelle association à partir de la demande
                var association = new Association
                {
                    Nom = demande.Nom,
                    Description = demande.Description,
                    Categorie = demande.Categorie,
                    Faculte = demande.Faculte,
                    CodePermanentCreateur = demande.CodePermanentCreateur,
                    DateCreation = DateTime.UtcNow,
                    Statut = StatutAssociation.EnAttente,
                    Objectifs = new List<string>(),
                    Activites = new List<string>(),
                    Membres = new List<Membre>(),
                    Evenements = new List<Evenement>()
                };

                // Ajouter le créateur comme président de l'association
                var membreCreateur = new Membre
                {
                    CodePermanent = demande.CodePermanentCreateur,
                    Role = RoleMembre.President,
                    DateAdhesion = DateTime.UtcNow,
                    Statut = StatutMembre.Actif,
                    Responsabilites = new List<string>(),
                    Associations = new List<string>()
                };

                association.Membres.Add(membreCreateur);

                // Sauvegarder l'association dans la base de données
                _context.Associations.Add(association);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Nouvelle association créée : {association.Nom} par l'utilisateur {demande.CodePermanentCreateur}");

                return CreatedAtAction(nameof(ObtenirAssociation), new { id = association.Id }, association);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de l'association");
                return StatusCode(500, "Une erreur est survenue lors de la création de l'association");
            }
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