using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "authentification")]
    public class AuthentificationController : ControllerBase
    {
        // Ce contrôleur sert uniquement pour Swagger
        // Les vraies requêtes sont routées par Ocelot
        
        /// <summary>
        /// Inscrire un nouvel utilisateur
        /// </summary>
        [HttpPost("inscription")]
        public IActionResult Inscription()
        {
            return Ok("Proxy - Authentification Service");
        }

        /// <summary>
        /// Se connecter
        /// </summary>
        [HttpPost("connexion")]
        public IActionResult Connexion()
        {
            return Ok("Proxy - Authentification Service");
        }

        /// <summary>
        /// Obtenir un utilisateur par code permanent
        /// </summary>
        [HttpGet("utilisateurs/{codePermanent}")]
        public IActionResult GetUtilisateur(string codePermanent)
        {
            return Ok("Proxy - Authentification Service");
        }

        /// <summary>
        /// Mettre à jour le profil utilisateur
        /// </summary>
        [HttpPut("profil")]
        public IActionResult UpdateProfil()
        {
            return Ok("Proxy - Authentification Service");
        }

        /// <summary>
        /// Changer le mot de passe
        /// </summary>
        [HttpPost("changement-mot-de-passe")]
        public IActionResult ChangementMotDePasse()
        {
            return Ok("Proxy - Authentification Service");
        }
    }
} 