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
        public IActionResult Test()
        {
            _logger.LogInformation("Test de l'API d'authentification");
            return Ok(new { message = "Service d'authentification opérationnel" });
        }

        [HttpPost("Inscription")]
        public async Task<IActionResult> Inscription([FromBody] DemandeInscription demande)
        {
            try
            {
                _logger.LogInformation("[Inscription] Début de la méthode pour l'utilisateur {Email}", demande.Email);
                Console.WriteLine($"[Inscription] Début de la méthode pour l'utilisateur {demande.Email}");

                // Vérifier si l'utilisateur existe déjà
                _logger.LogInformation("[Inscription] Vérification de l'existence de l'utilisateur {Email} ou code permanent {CodePermanent}", demande.Email, demande.CodePermanent);
                Console.WriteLine($"[Inscription] Vérification de l'existence de l'utilisateur {demande.Email} ou code permanent {demande.CodePermanent}");
                if (await _context.Utilisateurs.AnyAsync(u => u.Email == demande.Email || u.CodePermanent == demande.CodePermanent))
                {
                    _logger.LogWarning("[Inscription] Utilisateur déjà existant : {Email} ou {CodePermanent}", demande.Email, demande.CodePermanent);
                    Console.WriteLine($"[Inscription] Utilisateur déjà existant : {demande.Email} ou {demande.CodePermanent}");
                    return BadRequest(new { message = "Un utilisateur avec cet email ou ce code permanent existe déjà." });
                }

                // Créer le hash du mot de passe
                _logger.LogInformation("[Inscription] Création du hash du mot de passe pour {Email}", demande.Email);
                Console.WriteLine($"[Inscription] Création du hash du mot de passe pour {demande.Email}");
                var (motDePasseHash, motDePasseSalt) = _authService.HasherMotDePasse(demande.MotDePasse);

                // Créer le nouvel utilisateur
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
                    Role = demande.Role,
                    Programme = demande.Programme,
                    Faculte = demande.Faculte,
                    AnneeDebut = demande.AnneeDebut,
                    AnneeFin = demande.AnneeFin,
                    Telephone = demande.Telephone,
                    DateInscription = DateTime.UtcNow,
                    DerniereConnexion = DateTime.UtcNow
                };

                // Créer l'adresse
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

                // Ajouter l'utilisateur et l'adresse à la base de données
                _logger.LogInformation("[Inscription] Ajout de l'utilisateur et de l'adresse à la base de données : {Email}", demande.Email);
                Console.WriteLine($"[Inscription] Ajout de l'utilisateur et de l'adresse à la base de données : {demande.Email}");
                
                // Sauvegarder dans une transaction
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // D'abord sauvegarder l'utilisateur
                    await _context.Utilisateurs.AddAsync(nouvelUtilisateur);
                    await _context.SaveChangesAsync();

                    // Ensuite sauvegarder l'adresse
                    await _context.Adresses.AddAsync(adresse);
                    await _context.SaveChangesAsync();

                    // Valider la transaction
                    await transaction.CommitAsync();
                }
                catch
                {
                    // En cas d'erreur, annuler la transaction
                    await transaction.RollbackAsync();
                    throw;
                }

                // Générer le token JWT
                _logger.LogInformation("[Inscription] Génération du token JWT pour {Email}", demande.Email);
                Console.WriteLine($"[Inscription] Génération du token JWT pour {demande.Email}");
                var reponseToken = _jwtToken.GenererToken(nouvelUtilisateur);
                reponseToken.Message = "Inscription réussie";

                // Envoyer une notification de bienvenue
                // TODO: Envoyer une notification de bienvenue

                _logger.LogInformation("[Inscription] Inscription terminée avec succès pour {Email}", demande.Email);
                Console.WriteLine($"[Inscription] Inscription terminée avec succès pour {demande.Email}");

                return Ok(reponseToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Inscription] Erreur lors de l'inscription pour {Email}", demande?.Email);
                return StatusCode(500, new {
                    message = "Une erreur est survenue lors de l'inscription.",
                    erreur = ex.Message,
                    details = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpPost("Connexion")]
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

                // Mettre à jour la dernière connexion et activer l'utilisateur
                utilisateur.DerniereConnexion = DateTime.UtcNow;
                utilisateur.EstActif = true;
                await _context.SaveChangesAsync();

                // Envoyer une notification de connexion
                // TODO: Envoyer une notification de connexion

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
        public async Task<IActionResult> RafraichirToken([FromBody] string refreshToken)
        {
            try
            {
                _logger.LogInformation("[RafraichirToken] Début de la méthode");
                Console.WriteLine("[RafraichirToken] Début de la méthode");

                // Vérifier si le token est valide
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

                    // Vérifier si l'utilisateur existe et est actif
                    var utilisateur = await _context.Utilisateurs
                        .FirstOrDefaultAsync(u => u.CodePermanent == codePermanent && u.EstActif);

                    if (utilisateur == null)
                    {
                        _logger.LogWarning("[RafraichirToken] Utilisateur non trouvé ou inactif : {CodePermanent}", codePermanent);
                        return Unauthorized(new { message = "Token invalide ou utilisateur inactif" });
                    }

                    // Mettre à jour la dernière connexion
                    utilisateur.DerniereConnexion = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    // Générer un nouveau token
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
        public async Task<IActionResult> Deconnexion([FromBody] string? codePermanent = null)
        {
            try
            {
                _logger.LogInformation("[Deconnexion] Début de la méthode");
                
                string utilisateurCodePermanent = codePermanent;

                // Si aucun code permanent n'est fourni, essayer de le récupérer depuis le token
                if (string.IsNullOrEmpty(utilisateurCodePermanent))
                {
                    // Vérifier si l'utilisateur est authentifié via token
                    if (User.Identity?.IsAuthenticated == true)
                    {
                        utilisateurCodePermanent = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        if (string.IsNullOrEmpty(utilisateurCodePermanent))
                        {
                            _logger.LogWarning("[Deconnexion] Code permanent non trouvé dans le token");
                            return Unauthorized(new { message = "Token invalide ou code permanent manquant" });
                        }
                    }
                    else
                    {
                        _logger.LogWarning("[Deconnexion] Aucun code permanent fourni et utilisateur non authentifié");
                        return BadRequest(new { message = "Code permanent requis ou token d'authentification valide" });
                    }
                }

                // Récupérer l'utilisateur
                var utilisateur = await _context.Utilisateurs
                    .FirstOrDefaultAsync(u => u.CodePermanent == utilisateurCodePermanent);

                if (utilisateur == null)
                {
                    _logger.LogWarning("[Deconnexion] Utilisateur non trouvé : {CodePermanent}", utilisateurCodePermanent);
                    return NotFound(new { message = "Utilisateur non trouvé" });
                }

                // Vérifier si l'utilisateur est déjà déconnecté
                if (!utilisateur.EstActif)
                {
                    _logger.LogInformation("[Deconnexion] Utilisateur déjà déconnecté : {CodePermanent}", utilisateurCodePermanent);
                    return Ok(new { 
                        message = "Utilisateur déjà déconnecté",
                        codePermanent = utilisateurCodePermanent,
                        estConnecte = false
                    });
                }

                // Désactiver l'utilisateur et mettre à jour la dernière connexion
                utilisateur.EstActif = false;
                utilisateur.DerniereConnexion = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("[Deconnexion] Déconnexion réussie pour {CodePermanent}", utilisateurCodePermanent);
                return Ok(new { 
                    message = "Déconnexion réussie",
                    codePermanent = utilisateurCodePermanent,
                    estConnecte = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Deconnexion] Erreur lors de la déconnexion");
                return StatusCode(500, new { message = "Une erreur est survenue lors de la déconnexion" });
            }
        }

        [HttpGet("ObtenirProfilUtilisateur")]
        [Authorize]
        public async Task<IActionResult> ObtenirProfil()
        {
            try
            {
                _logger.LogInformation("[ObtenirProfil] Début de la méthode");
                
                // Récupérer le code permanent de l'utilisateur depuis le token
                var codePermanent = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(codePermanent))
                {
                    _logger.LogWarning("[ObtenirProfil] Code permanent non trouvé dans le token");
                    return Unauthorized(new { message = "Token invalide" });
                }

                // Récupérer l'utilisateur avec son adresse
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Adresses)
                    .FirstOrDefaultAsync(u => u.CodePermanent == codePermanent);

                if (utilisateur == null)
                {
                    _logger.LogWarning("[ObtenirProfil] Utilisateur non trouvé : {CodePermanent}", codePermanent);
                    return NotFound(new { message = "Utilisateur non trouvé" });
                }

                // Créer la réponse avec les informations nécessaires
                var profil = new
                {
                    // Informations de base
                    CodePermanent = utilisateur.CodePermanent,
                    Email = utilisateur.Email,
                    Nom = utilisateur.Nom,
                    Prenom = utilisateur.Prenom,
                    Telephone = utilisateur.Telephone,
                    Role = utilisateur.Role,

                    // Informations académiques
                    Programme = utilisateur.Programme,
                    Faculte = utilisateur.Faculte,
                    AnneeDebut = utilisateur.AnneeDebut,
                    AnneeFin = utilisateur.AnneeFin,

                    // Informations de l'adresse
                    Adresse = utilisateur.Adresses.FirstOrDefault() != null ? new
                    {
                        NumeroCivique = utilisateur.Adresses.First().NumeroCivique,
                        Rue = utilisateur.Adresses.First().Rue,
                        Appartement = utilisateur.Adresses.First().Appartement,
                        Ville = utilisateur.Adresses.First().Ville,
                        Province = utilisateur.Adresses.First().Province,
                        CodePostal = utilisateur.Adresses.First().CodePostal
                    } : null,

                    // Informations de dates
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
        public async Task<IActionResult> ModifierProfil([FromBody] DemandeMiseAJourProfil demande)
        {
            try
            {
                _logger.LogInformation("[ModifierProfil] Début de la méthode");
                
                // Récupérer le code permanent de l'utilisateur depuis le token
                var codePermanent = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(codePermanent))
                {
                    _logger.LogWarning("[ModifierProfil] Code permanent non trouvé dans le token");
                    return Unauthorized(new { message = "Token invalide" });
                }

                // Récupérer l'utilisateur avec son adresse
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Adresses)
                    .FirstOrDefaultAsync(u => u.CodePermanent == codePermanent);

                if (utilisateur == null)
                {
                    _logger.LogWarning("[ModifierProfil] Utilisateur non trouvé : {CodePermanent}", codePermanent);
                    return NotFound(new { message = "Utilisateur non trouvé" });
                }

                // Vérifier si l'email est déjà utilisé par un autre utilisateur
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

                // Mettre à jour les informations de base
                utilisateur.Email = demande.Email;
                utilisateur.Telephone = demande.Telephone;
                utilisateur.Nom = demande.Nom;
                utilisateur.Prenom = demande.Prenom;
                utilisateur.Programme = demande.Programme;
                utilisateur.Faculte = demande.Faculte;
                
                // Conversion explicite des années nullable en int
                if (demande.AnneeDebut.HasValue)
                    utilisateur.AnneeDebut = demande.AnneeDebut.Value;
                if (demande.AnneeFin.HasValue)
                    utilisateur.AnneeFin = demande.AnneeFin.Value;

                // Mettre à jour l'adresse
                if (demande.Adresse != null)
                {
                    var adresseExistante = utilisateur.Adresses.FirstOrDefault();
                    if (adresseExistante != null)
                    {
                        // Mettre à jour l'adresse existante
                        adresseExistante.NumeroCivique = demande.Adresse.NumeroCivique;
                        adresseExistante.Rue = demande.Adresse.Rue;
                        adresseExistante.Appartement = demande.Adresse.Appartement;
                        adresseExistante.Ville = demande.Adresse.Ville;
                        adresseExistante.Province = demande.Adresse.Province;
                        adresseExistante.CodePostal = demande.Adresse.CodePostal;
                    }
                    else
                    {
                        // Créer une nouvelle adresse
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

                // Marquer l'entité comme modifiée
                _context.Entry(utilisateur).State = EntityState.Modified;
                if (utilisateur.Adresses.Any())
                {
                    _context.Entry(utilisateur.Adresses.First()).State = EntityState.Modified;
                }

                // Sauvegarder les modifications
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
        public async Task<IActionResult> ChangerMotDePasse([FromBody] DemandeChangementMotDePasse demande)
        {
            try
            {
                _logger.LogInformation("[ChangerMotDePasse] Début de la méthode");
                
                // Récupérer le code permanent de l'utilisateur depuis le token
                var codePermanent = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(codePermanent))
                {
                    _logger.LogWarning("[ChangerMotDePasse] Code permanent non trouvé dans le token");
                    return Unauthorized(new { message = "Token invalide" });
                }

                // Vérifier que les mots de passe correspondent
                if (demande.NouveauMotDePasse != demande.ConfirmationMotDePasse)
                {
                    _logger.LogWarning("[ChangerMotDePasse] Les mots de passe ne correspondent pas");
                    return BadRequest(new { message = "Le nouveau mot de passe et la confirmation ne correspondent pas" });
                }

                // Vérifier que le nouveau mot de passe est différent de l'ancien
                if (demande.NouveauMotDePasse == demande.AncienMotDePasse)
                {
                    _logger.LogWarning("[ChangerMotDePasse] Le nouveau mot de passe est identique à l'ancien");
                    return BadRequest(new { message = "Le nouveau mot de passe doit être différent de l'ancien" });
                }

                // Récupérer l'utilisateur
                var utilisateur = await _context.Utilisateurs
                    .FirstOrDefaultAsync(u => u.CodePermanent == codePermanent);

                if (utilisateur == null)
                {
                    _logger.LogWarning("[ChangerMotDePasse] Utilisateur non trouvé : {CodePermanent}", codePermanent);
                    return NotFound(new { message = "Utilisateur non trouvé" });
                }

                // Vérifier l'ancien mot de passe
                if (!_authService.VerifierMotDePasse(demande.AncienMotDePasse, utilisateur.MotDePasseHash, utilisateur.MotDePasseSalt))
                {
                    _logger.LogWarning("[ChangerMotDePasse] Ancien mot de passe incorrect pour {CodePermanent}", codePermanent);
                    return BadRequest(new { message = "L'ancien mot de passe est incorrect" });
                }

                // Générer le nouveau hash du mot de passe
                var (nouveauHash, nouveauSalt) = _authService.HasherMotDePasse(demande.NouveauMotDePasse);

                // Mettre à jour le mot de passe
                utilisateur.MotDePasseHash = nouveauHash;
                utilisateur.MotDePasseSalt = nouveauSalt;
                await _context.SaveChangesAsync();

                // Envoyer une notification de changement de mot de passe
                // TODO: Envoyer une notification de changement de mot de passe

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
        public async Task<IActionResult> VerifierUtilisateur(string codePermanent)
        {
            try
            {
                _logger.LogInformation("[VerifierUtilisateur] Vérification de l'utilisateur {CodePermanent}", codePermanent);

                // Vérifier si l'utilisateur existe dans la base de données
                var utilisateur = await _context.Utilisateurs
                    .FirstOrDefaultAsync(u => u.CodePermanent == codePermanent);

                if (utilisateur == null)
                {
                    _logger.LogWarning("[VerifierUtilisateur] Utilisateur non trouvé : {CodePermanent}", codePermanent);
                    return NotFound(new { 
                        message = $"Utilisateur avec le code permanent {codePermanent} non trouvé",
                        codePermanent = codePermanent,
                        existe = false,
                        estConnecte = false
                    });
                }

                // Vérifier si l'utilisateur est connecté (actif)
                if (!utilisateur.EstActif)
                {
                    _logger.LogWarning("[VerifierUtilisateur] Utilisateur non connecté : {CodePermanent}", codePermanent);
                    return BadRequest(new { 
                        message = $"L'utilisateur {codePermanent} n'est pas connecté",
                        codePermanent = codePermanent,
                        existe = true,
                        estConnecte = false,
                        derniereConnexion = utilisateur.DerniereConnexion
                    });
                }

                // L'utilisateur existe et est connecté
                var reponse = new
                {
                    message = "Utilisateur connecté et autorisé",
                    codePermanent = utilisateur.CodePermanent,
                    nom = utilisateur.Nom,
                    prenom = utilisateur.Prenom,
                    email = utilisateur.Email,
                    role = utilisateur.Role,
                    existe = true,
                    estConnecte = true,
                    derniereConnexion = utilisateur.DerniereConnexion
                };

                _logger.LogInformation("[VerifierUtilisateur] Utilisateur connecté et autorisé : {CodePermanent}", codePermanent);

                return Ok(reponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[VerifierUtilisateur] Erreur lors de la vérification de l'utilisateur {CodePermanent}", codePermanent);
                return StatusCode(500, new { 
                    message = "Une erreur est survenue lors de la vérification de l'utilisateur",
                    codePermanent = codePermanent,
                    existe = false,
                    estConnecte = false
                });
            }
        }
    }
} 