using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [ApiController]
    [Route("market")]
    [ApiExplorerSettings(GroupName = "market")]
    public class MarketController : ControllerBase
    {
        // Ce contrôleur sert uniquement pour Swagger
        // Les vraies requêtes sont routées par Ocelot
        
        /// <summary>
        /// Obtenir tous les produits
        /// </summary>
        [HttpGet("produits")]
        public IActionResult GetProduits()
        {
            return Ok("Proxy - Market Service");
        }

        /// <summary>
        /// Obtenir un produit par ID
        /// </summary>
        [HttpGet("produits/{id}")]
        public IActionResult GetProduit(int id)
        {
            return Ok("Proxy - Market Service");
        }

        /// <summary>
        /// Créer un nouveau produit
        /// </summary>
        [HttpPost("produits")]
        public IActionResult CreateProduit()
        {
            return Ok("Proxy - Market Service");
        }

        /// <summary>
        /// Mettre à jour un produit
        /// </summary>
        [HttpPut("produits/{id}")]
        public IActionResult UpdateProduit(int id)
        {
            return Ok("Proxy - Market Service");
        }

        /// <summary>
        /// Supprimer un produit
        /// </summary>
        [HttpDelete("produits/{id}")]
        public IActionResult DeleteProduit(int id)
        {
            return Ok("Proxy - Market Service");
        }

        /// <summary>
        /// Obtenir toutes les catégories
        /// </summary>
        [HttpGet("categories")]
        public IActionResult GetCategories()
        {
            return Ok("Proxy - Market Service");
        }

        /// <summary>
        /// Créer une nouvelle catégorie
        /// </summary>
        [HttpPost("categories")]
        public IActionResult CreateCategorie()
        {
            return Ok("Proxy - Market Service");
        }
    }
} 