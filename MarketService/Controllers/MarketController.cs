using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarketService.Models;
using MarketService.Data;

namespace MarketService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class MarketController : ControllerBase
    {
        private readonly MarketDbContext _context;
        private readonly ILogger<MarketController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public MarketController(
            MarketDbContext context, 
            ILogger<MarketController> logger,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        // --- Gestion des produits ---
        [HttpGet("produits")]
        public async Task<ActionResult<IEnumerable<Produit>>> GetProduits(
            [FromQuery] int? categorieId = null,
            [FromQuery] StatutProduit? statut = null,
            [FromQuery] string? vendeur = null)
        {
            var query = _context.Produits.Include(p => p.Categorie).AsQueryable();

            if (categorieId.HasValue)
                query = query.Where(p => p.CategorieId == categorieId.Value);

            if (statut.HasValue)
                query = query.Where(p => p.Statut == statut.Value);

            if (!string.IsNullOrEmpty(vendeur))
                query = query.Where(p => p.CodePermanentVendeur == vendeur);

            var produits = await query.ToListAsync();
            return Ok(produits);
        }

        [HttpGet("produits/{id}")]
        public async Task<ActionResult<Produit>> GetProduit(int id)
        {
            var produit = await _context.Produits
                .Include(p => p.Categorie)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produit == null)
                return NotFound(new { message = "Produit non trouvé" });

            return Ok(produit);
        }

        [HttpPost("produits")]
        public async Task<ActionResult<Produit>> CreateProduit([FromBody] Produit produit)
        {
            // Vérifier que la catégorie existe
            var categorie = await _context.Categories.FindAsync(produit.CategorieId);
            if (categorie == null)
                return BadRequest(new { message = "Catégorie invalide" });

            // Vérifier que l'utilisateur existe dans le service d'authentification
            var client = _httpClientFactory.CreateClient("AuthService");
            var response = await client.GetAsync($"api/authentification/VerifierUtilisateur/{produit.CodePermanentVendeur}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Utilisateur non trouvé : {CodePermanent}, Status: {Status}", 
                    produit.CodePermanentVendeur, response.StatusCode);
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return BadRequest(new { message = "Le code permanent du vendeur n'existe pas dans le système d'authentification" });
                }
                else
                {
                    return BadRequest(new { message = "Erreur lors de la vérification de l'utilisateur" });
                }
            }

            produit.DateCreation = DateTime.UtcNow;
            produit.Statut = StatutProduit.Disponible;

            _context.Produits.Add(produit);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Nouveau produit créé: {Titre} par {Vendeur}", produit.Titre, produit.CodePermanentVendeur);

            return CreatedAtAction(nameof(GetProduit), new { id = produit.Id }, produit);
        }

        [HttpPut("produits/{id}")]
        public async Task<IActionResult> UpdateProduit(int id, [FromBody] Produit produit)
        {
            var existing = await _context.Produits.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Produit non trouvé" });

            existing.Titre = produit.Titre;
            existing.Description = produit.Description;
            existing.Prix = produit.Prix;
            existing.ModePaiementSouhaite = produit.ModePaiementSouhaite;
            existing.ContactVendeur = produit.ContactVendeur;
            existing.Localisation = produit.Localisation;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("produits/{id}/statut")]
        public async Task<IActionResult> UpdateStatutProduit(int id, [FromBody] StatutProduit nouveauStatut)
        {
            var produit = await _context.Produits.FindAsync(id);
            if (produit == null)
                return NotFound(new { message = "Produit non trouvé" });

            produit.Statut = nouveauStatut;
            
            if (nouveauStatut == StatutProduit.Vendu)
                produit.DateVente = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("produits/{id}")]
        public async Task<IActionResult> DeleteProduit(int id)
        {
            var produit = await _context.Produits.FindAsync(id);
            if (produit == null)
                return NotFound(new { message = "Produit non trouvé" });

            _context.Produits.Remove(produit);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // --- Gestion des catégories ---
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<Categorie>>> GetCategories()
        {
            var categories = await _context.Categories
                .Include(c => c.Produits)
                .Where(c => c.EstActive)
                .ToListAsync();

            return Ok(categories);
        }

        [HttpGet("categories/{id}")]
        public async Task<ActionResult<Categorie>> GetCategorie(int id)
        {
            var categorie = await _context.Categories
                .Include(c => c.Produits)
                .FirstOrDefaultAsync(c => c.Id == id && c.EstActive);

            if (categorie == null)
                return NotFound(new { message = "Catégorie non trouvée" });

            return Ok(categorie);
        }

        [HttpPost("categories")]
        public async Task<ActionResult<Categorie>> CreateCategorie([FromBody] Categorie categorie)
        {
            categorie.EstActive = true;
            _context.Categories.Add(categorie);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategorie), new { id = categorie.Id }, categorie);
        }

        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategorie(int id, [FromBody] Categorie categorie)
        {
            var existing = await _context.Categories.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Catégorie non trouvée" });

            existing.Nom = categorie.Nom;
            existing.Description = categorie.Description;
            existing.EstActive = categorie.EstActive;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
} 