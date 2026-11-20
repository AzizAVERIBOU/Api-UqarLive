using Microsoft.AspNetCore.Mvc;
using AuthentificationService.Models;
using Microsoft.Extensions.Logging;
using AuthentificationService.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using AuthentificationService.Data;
using Microsoft.EntityFrameworkCore;
using RessourcesPartagees.Enumerations;
using RessourcesPartagees;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthentificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthentificationController : ControllerBase
    {
        private readonly ILogger<AuthentificationController> _logger;
        private readonly JwtToken _jwtToken;
        private readonly AuthentificationDbContext _context;
        private readonly ServiceAuthentification _authService;
        private readonly IConfiguration _configuration;

        public AuthentificationController(
            ILogger<AuthentificationController> logger, 
            JwtToken jwtToken,
            AuthentificationDbContext context,
            ServiceAuthentification authService,
            IConfiguration configuration)
        {
            _logger = logger;
            _jwtToken = jwtToken;
            _context = context;
            _authService = authService;
            _configuration = configuration;
        }

        [HttpGet("test")]
        [ProducesResponseType(typeof(MessageApi), StatusCodes.Status200OK)]
        public IActionResult Test()
        {
            _logger.LogInformation("Test de l'API d'authentification");
            return Ok(new { message = "Service d'authentification opérationnel" });
        }

        [HttpPost("Inscription")]
        [ProducesResponseType(typeof(ReponseToken), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageApi), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Inscription([FromBody] DemandeInscription demande)
        {
            try
            {
                _logger.LogInformation("[Inscription] Début de la méthode pour l'utilisateur {Email}", demande.Email);
                Console.WriteLine($"[Inscription] Début de la méthode pour l'utilisateur {demande.Email}");

                if (string.IsNullOrWhiteSpace(demande.MotDePasse) || demande.MotDePasse.Length < 8)
                {
                    return BadRequest(new { message = "Le mot de passe doit contenir au moins 8 caractères." });
                }

                if (string.IsNullOrWhiteSpace(demande.Telephone))
                {
                    return BadRequest(new { message = "Le numéro de téléphone est requis." });
                }

                if (demande.Adresse == null
                    || string.IsNullOrWhiteSpace(demande.Adresse.NumeroCivique)
                    || string.IsNullOrWhiteSpace(demande.Adresse.Rue)
                    || string.IsNullOrWhiteSpace(demande.Adresse.Ville)
                    || string.IsNullOrWhiteSpace(demande.Adresse.Province)
                    || string.IsNullOrWhiteSpace(demande.Adresse.CodePostal))
                {
                    return BadRequest(new { message = "L'adresse est requise." });
                }

                _logger.LogInformation("[Inscription] Vérification de l'existence de l'utilisateur {Email} ou code permanent {CodePermanent}", demande.Email, demande.CodePermanent);
                Console.WriteLine($"[Inscription] Vérification de l'existence de l'utilisateur {demande.Email} ou code permanent {demande.CodePermanent}");
                if (await _context.Utilisateurs.AnyAsync(u => u.Email == demande.Email || u.CodePermanent == demande.CodePermanent))
                {
                    _logger.LogWarning("[Inscription] Utilisateur déjà existant : {Email} ou {CodePermanent}", demande.Email, demande.CodePermanent);
                    Console.WriteLine($"[Inscription] Utilisateur déjà existant : {demande.Email} ou {demande.CodePermanent}");
                    return BadRequest(new { message = "Un utilisateur avec cet email ou ce code permanent existe déjà." });
                }

                _logger.LogInformation("[Inscription] Création du hash du mot de passe pour {Email}", demande.Email);
                Console.WriteLine($"[Inscription] Création du hash du mot de passe pour {demande.Email}");
                var (motDePasseHash, motDePasseSalt) = _authService.HasherMotDePasse(demande.MotDePasse);

                _logger.LogInformation("[Inscription] Création de l'objet Utilisateur pour {Email}", demande.Email);
                Console.WriteLine($"[Inscription] Création de l'objet Utilisateur pour {demande.Email}");
                var nouvelUtilisateur = new Utilisateur
                {
                    CodePermanent = demande.CodePermanent,
                    Email = demande.Email,
                    Nom = demande.Nom,
                    Prenom = demande.Prenom,
                    MotDePasseHash = motDePasseHash,
                    MotDePasseSalt = motDePasseSalt,
                    Role = RoleUtilisateur.Etudiant,
                    Programme = demande.Programme,
                    Faculte = demande.Faculte,
                    AnneeDebut = demande.AnneeDebut,
                    AnneeFin = demande.AnneeFin,
                    Telephone = demande.Telephone,
                    DateInscription = DateTime.UtcNow,
                    DateCreation = DateTime.UtcNow,
                    DerniereConnexion = DateTime.UtcNow,
                    EstActif = true
                };

                var adresse = new Adresse
                {
                    NumeroCivique = demande.Adresse.NumeroCivique,
                    Rue = demande.Adresse.Rue,
                    Appartement = demande.Adresse.Appartement,
                    Ville = demande.Adresse.Ville,
                    Province = demande.Adresse.Province,
                    CodePostal = demande.Adresse.CodePostal,
                    CodePermanentUtilisateur = demande.CodePermanent
                };

                _logger.LogInformation("[Inscription] Ajout de l'utilisateur et de l'adresse à la base de données : {Email}", demande.Email);
                Console.WriteLine($"[Inscription] Ajout de l'utilisateur et de l'adresse à la base de données : {demande.Email}");
                
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    await _context.Utilisateurs.AddAsync(nouvelUtilisateur);
                    await _context.SaveChangesAsync();

                    await _context.Adresses.AddAsync(adresse);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }

                _logger.LogInformation("[Inscription] Génération du token JWT pour {Email}", demande.Email);
                Console.WriteLine($"[Inscription] Génération du token JWT pour {demande.Email}");
                var reponseToken = _jwtToken.GenererToken(nouvelUtilisateur);
                reponseToken.Message = "Inscription réussie";

                _logger.LogInformation("[Inscription] Inscription terminée avec succès pour {Email}", demande.Email);
                Console.WriteLine($"[Inscription] Inscription terminée avec succès pour {demande.Email}");

                return Ok(reponseToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Inscription] Erreur lors de l'inscription pour {Email}", demande?.Email);
                return StatusCode(500, new { message = "Une erreur est survenue lors de l'inscription." });
            }
        }

        [HttpPost("Connexion")]
        [ProducesResponseType(typeof(ReponseToken), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageApi), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Connexion([FromBody] DemandeConnexion demande)
        {
            try
            {
                _logger.LogInformation("Tentative de connexion pour l'utilisateur {CodePermanent}", demande.CodePermanent);

                var utilisateur = await _authService.AuthentifierUtilisateur(demande.CodePermanent, demande.MotDePasse);
                if (utilisateur == null)
                {
                    return Unauthorized(new { message = "Code permanent ou mot de passe incorrect" });
                }

                if (!utilisateur.EstActif)
                {
                    return Unauthorized(new { message = "Ce compte est désactivé" });
                }

                utilisateur.DerniereConnexion = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var reponseToken = _jwtToken.GenererToken(utilisateur);
                reponseToken.Message = "Connexion réussie";
                return Ok(reponseToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la connexion");
                return StatusCode(500, new { message = "Une erreur est survenue lors de la connexion." });
            }
        }

        [HttpPost("RafraichirToken")]
        [ProducesResponseType(typeof(ReponseToken), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageApi), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RafraichirToken([FromBody] string refreshToken)
        {
            try
            {
                _logger.LogInformation("[RafraichirToken] Début de la méthode");
                Console.WriteLine("[RafraichirToken] Début de la méthode");

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);

                try
                {
                    tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    var jwtToken = (JwtSecurityToken)validatedToken;
                    var codePermanent = jwtToken.Claims.First(x => x.Type == "nameid").Value;

                    var utilisateur = await _context.Utilisateurs
                        .FirstOrDefaultAsync(u => u.CodePermanent == codePermanent && u.EstActif);

                    if (utilisateur == null)
                    {
                        _logger.LogWarning("[RafraichirToken] Utilisateur non trouvé ou inactif : {CodePermanent}", codePermanent);
                        return Unauthorized(new { message = "Token invalide ou utilisateur inactif" });
                    }

                    utilisateur.DerniereConnexion = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    var reponseToken = _jwtToken.GenererToken(utilisateur);
                    reponseToken.Message = "Token rafraîchi avec succès";

                    _logger.LogInformation("[RafraichirToken] Token rafraîchi avec succès pour {CodePermanent}", codePermanent);
                    return Ok(reponseToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("[RafraichirToken] Token invalide : {Message}", ex.Message);
                    return Unauthorized(new { message = "Token invalide" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RafraichirToken] Erreur lors du rafraîchissement du token");
                return StatusCode(500, new { message = "Une erreur est survenue lors du rafraîchissement du token" });
            }
        }

        [HttpPost("Deconnexion")]
        [Authorize]
        [ProducesResponseType(typeof(DeconnexionApi), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageApi), StatusCodes.Status401Unauthorized)]
        public IActionResult Deconnexion()
        {
            var codePermanent = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(codePermanent))
            {
                _logger.LogWarning("[Deconnexion] Code permanent non trouvé dans le token");
                return Unauthorized(new { message = "Token invalide" });
            }

            _logger.LogInformation("[Deconnexion] Jeton à oublier pour {CodePermanent}", codePermanent);
            return Ok(new
            {
                message = "Déconnexion réussie. Le jeton doit être oublié par le client.",
                codePermanent
            });
        }

        [HttpGet("ObtenirProfilUtilisateur")]
        [Authorize]
        [ProducesResponseType(typeof(ProfilApi), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageApi), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ObtenirProfil()
        {
            try
            {
                _logger.LogInformation("[ObtenirProfil] Début de la méthode");
                
                var codePermanent = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(codePermanent))
                {
                    _logger.LogWarning("[ObtenirProfil] Code permanent non trouvé dans le token");
                    return Unauthorized(new { message = "Token invalide" });
                }

                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Adresses)
                    .FirstOrDefaultAsync(u => u.CodePermanent == codePermanent);

                if (utilisateur == null)
                {
                    _logger.LogWarning("[ObtenirProfil] Utilisateur non trouvé : {CodePermanent}", codePermanent);
                    return NotFound(new { message = "Utilisateur non trouvé" });
                }

                var profil = new
                {
                    CodePermanent = utilisateur.CodePermanent,
                    Email = utilisateur.Email,
                    Nom = utilisateur.Nom,
                    Prenom = utilisateur.Prenom,
                    Telephone = utilisateur.Telephone,
                    Role = utilisateur.Role,

                    Programme = utilisateur.Programme,
                    Faculte = utilisateur.Faculte,
                    AnneeDebut = utilisateur.AnneeDebut,
                    AnneeFin = utilisateur.AnneeFin,

                    Adresse = utilisateur.Adresses.FirstOrDefault() != null ? new
                    {
                        NumeroCivique = utilisateur.Adresses.First().NumeroCivique,
                        Rue = utilisateur.Adresses.First().Rue,
                        Appartement = utilisateur.Adresses.First().Appartement,
                        Ville = utilisateur.Adresses.First().Ville,
                        Province = utilisateur.Adresses.First().Province,
                        CodePostal = utilisateur.Adresses.First().CodePostal
                    } : null,

                    DateInscription = utilisateur.DateInscription,
                    DerniereConnexion = utilisateur.DerniereConnexion,
                    EstActif = utilisateur.EstActif
                };

                _logger.LogInformation("[ObtenirProfil] Profil récupéré avec succès pour {CodePermanent}", codePermanent);
                return Ok(profil);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ObtenirProfil] Erreur lors de la récupération du profil");
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération du profil" });
            }
        }

        [HttpPut("MettreAJourProfilUtilisateur")]
        [Authorize]
        [ProducesResponseType(typeof(MessageApi), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageApi), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MessageApi), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ModifierProfil([FromBody] DemandeMiseAJourProfil demande)
        {
            try
            {
                _logger.LogInformation("[ModifierProfil] Début de la méthode");
                
                var codePermanent = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(codePermanent))
                {
                    _logger.LogWarning("[ModifierProfil] Code permanent non trouvé dans le token");
                    return Unauthorized(new { message = "Token invalide" });
                }

                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Adresses)
                    .FirstOrDefaultAsync(u => u.CodePermanent == codePermanent);

                if (utilisateur == null)
                {
                    _logger.LogWarning("[ModifierProfil] Utilisateur non trouvé : {CodePermanent}", codePermanent);
                    return NotFound(new { message = "Utilisateur non trouvé" });
                }

                if (demande.Email != utilisateur.Email)
                {
                    var emailExiste = await _context.Utilisateurs
                        .AnyAsync(u => u.Email == demande.Email && u.CodePermanent != codePermanent);
                    
                    if (emailExiste)
                    {
                        _logger.LogWarning("[ModifierProfil] Email déjà utilisé : {Email}", demande.Email);
                        return BadRequest(new { message = "Cet email est déjà utilisé par un autre utilisateur" });
                    }
                }

                if (string.IsNullOrWhiteSpace(demande.Telephone))
                {
                    return BadRequest(new { message = "Le numéro de téléphone est requis." });
                }

                utilisateur.Email = demande.Email;
                utilisateur.Telephone = demande.Telephone;
                utilisateur.Nom = demande.Nom;
                utilisateur.Prenom = demande.Prenom;
                utilisateur.Programme = demande.Programme;
                utilisateur.Faculte = demande.Faculte;
                
                if (demande.AnneeDebut.HasValue)
                    utilisateur.AnneeDebut = demande.AnneeDebut.Value;
                if (demande.AnneeFin.HasValue)
                    utilisateur.AnneeFin = demande.AnneeFin.Value;

                if (demande.Adresse != null)
                {
                    var adresseExistante = utilisateur.Adresses.FirstOrDefault();
                    if (adresseExistante != null)
                    {
                        adresseExistante.NumeroCivique = demande.Adresse.NumeroCivique;
                        adresseExistante.Rue = demande.Adresse.Rue;
                        adresseExistante.Appartement = demande.Adresse.Appartement;
                        adresseExistante.Ville = demande.Adresse.Ville;
                        adresseExistante.Province = demande.Adresse.Province;
                        adresseExistante.CodePostal = demande.Adresse.CodePostal;
                    }
                    else
                    {
                        var nouvelleAdresse = new Adresse
                        {
                            NumeroCivique = demande.Adresse.NumeroCivique,
                            Rue = demande.Adresse.Rue,
                            Appartement = demande.Adresse.Appartement,
                            Ville = demande.Adresse.Ville,
                            Province = demande.Adresse.Province,
                            CodePostal = demande.Adresse.CodePostal,
                            CodePermanentUtilisateur = codePermanent
                        };
                        await _context.Adresses.AddAsync(nouvelleAdresse);
                    }
                }

                _context.Entry(utilisateur).State = EntityState.Modified;
                if (utilisateur.Adresses.Any())
                {
                    _context.Entry(utilisateur.Adresses.First()).State = EntityState.Modified;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("[ModifierProfil] Profil mis à jour avec succès pour {CodePermanent}", codePermanent);
                return Ok(new { message = "Profil mis à jour avec succès" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ModifierProfil] Erreur lors de la mise à jour du profil");
                return StatusCode(500, new { message = "Une erreur est survenue lors de la mise à jour du profil" });
            }
        }

        [HttpPost("ChangerMotDePasse")]
        [Authorize]
        [ProducesResponseType(typeof(MessageApi), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MessageApi), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MessageApi), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangerMotDePasse([FromBody] DemandeChangementMotDePasse demande)
        {
            try
            {
                _logger.LogInformation("[ChangerMotDePasse] Début de la méthode");
                
                var codePermanent = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(codePermanent))
                {
                    _logger.LogWarning("[ChangerMotDePasse] Code permanent non trouvé dans le token");
                    return Unauthorized(new { message = "Token invalide" });
                }

                if (string.IsNullOrWhiteSpace(demande.NouveauMotDePasse) || demande.NouveauMotDePasse.Length < 8)
                {
                    return BadRequest(new { message = "Le mot de passe doit contenir au moins 8 caractères." });
                }

                if (demande.NouveauMotDePasse != demande.ConfirmationMotDePasse)
                {
                    _logger.LogWarning("[ChangerMotDePasse] Les mots de passe ne correspondent pas");
                    return BadRequest(new { message = "Le nouveau mot de passe et la confirmation ne correspondent pas" });
                }

                if (demande.NouveauMotDePasse == demande.AncienMotDePasse)
                {
                    _logger.LogWarning("[ChangerMotDePasse] Le nouveau mot de passe est identique à l'ancien");
                    return BadRequest(new { message = "Le nouveau mot de passe doit être différent de l'ancien" });
                }

                var utilisateur = await _context.Utilisateurs
                    .FirstOrDefaultAsync(u => u.CodePermanent == codePermanent);

                if (utilisateur == null)
                {
                    _logger.LogWarning("[ChangerMotDePasse] Utilisateur non trouvé : {CodePermanent}", codePermanent);
                    return NotFound(new { message = "Utilisateur non trouvé" });
                }

                if (!_authService.VerifierMotDePasse(demande.AncienMotDePasse, utilisateur.MotDePasseHash, utilisateur.MotDePasseSalt))
                {
                    _logger.LogWarning("[ChangerMotDePasse] Ancien mot de passe incorrect pour {CodePermanent}", codePermanent);
                    return BadRequest(new { message = "L'ancien mot de passe est incorrect" });
                }

                var (nouveauHash, nouveauSalt) = _authService.HasherMotDePasse(demande.NouveauMotDePasse);

                utilisateur.MotDePasseHash = nouveauHash;
                utilisateur.MotDePasseSalt = nouveauSalt;
                await _context.SaveChangesAsync();

                _logger.LogInformation("[ChangerMotDePasse] Mot de passe changé avec succès pour {CodePermanent}", codePermanent);
                return Ok(new { message = "Mot de passe changé avec succès" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ChangerMotDePasse] Erreur lors du changement de mot de passe");
                return StatusCode(500, new { message = "Une erreur est survenue lors du changement de mot de passe" });
            }
        }

        [HttpGet("VerifierUtilisateur/{codePermanent}")]
        [ProducesResponseType(typeof(VerificationUtilisateurApi), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(VerificationUtilisateurApi), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VerifierUtilisateur(string codePermanent)
        {
            try
            {
                _logger.LogInformation("[VerifierUtilisateur] Vérification de l'utilisateur {CodePermanent}", codePermanent);

                var utilisateur = await _context.Utilisateurs
                    .FirstOrDefaultAsync(u => u.CodePermanent == codePermanent);

                if (utilisateur == null || !utilisateur.EstActif)
                {
                    _logger.LogWarning("[VerifierUtilisateur] Utilisateur introuvable ou inactif : {CodePermanent}", codePermanent);
                    return NotFound(new
                    {
                        message = $"Utilisateur avec le code permanent {codePermanent} non trouvé",
                        codePermanent,
                        existe = false
                    });
                }

                var reponse = new
                {
                    message = "Utilisateur actif",
                    codePermanent = utilisateur.CodePermanent,
                    nom = utilisateur.Nom,
                    prenom = utilisateur.Prenom,
                    email = utilisateur.Email,
                    role = utilisateur.Role,
                    existe = true
                };

                _logger.LogInformation("[VerifierUtilisateur] Utilisateur actif : {CodePermanent}", codePermanent);

                return Ok(reponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[VerifierUtilisateur] Erreur lors de la vérification de l'utilisateur {CodePermanent}", codePermanent);
                return StatusCode(500, new
                {
                    message = "Une erreur est survenue lors de la vérification de l'utilisateur",
                    codePermanent,
                    existe = false
                });
            }
        }
    }
} 
