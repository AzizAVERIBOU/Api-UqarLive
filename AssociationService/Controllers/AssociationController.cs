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
        private const int ROLE_ADMIN = 2; // RoleUtilisateur.Admin

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
            try
            {
                _logger.LogInformation("[ObtenirAssociations] Récupération de toutes les associations");

                // Récupérer toutes les associations avec leurs membres et événements
                var associations = await _context.Associations
                    .Include(a => a.Membres)
                    .Include(a => a.Evenements)
                    .Select(a => new
                    {
                        a.Id,
                        a.Nom,
                        a.Description,
                        a.Categorie,
                        a.Faculte,
                        a.CodePermanentCreateur,
                        a.DateCreation,
                        a.Statut,
                        Objectifs = a.Objectifs ?? new List<string>(),
                        Activites = a.Activites ?? new List<string>(),
                        Membres = a.Membres.Select(m => new
                        {
                            m.Id,
                            m.CodePermanent,
                            m.Role,
                            m.DateAdhesion,
                            m.Statut,
                            Responsabilites = m.Responsabilites ?? new List<string>()
                        }).ToList(),
                        Evenements = a.Evenements.Select(e => new
                        {
                            e.Id,
                            e.Titre,
                            e.Description,
                            e.DateDebut,
                            e.DateFin,
                            e.Lieu,
                            e.CapaciteMax,
                            Participants = e.Participants ?? new List<string>(),
                            e.Statut
                        }).ToList()
                    })
                    .ToListAsync();

                if (!associations.Any())
                {
                    _logger.LogInformation("[ObtenirAssociations] Aucune association trouvée");
                    return Ok(new { 
                        message = "Aucune association n'existe actuellement",
                        associations = new List<object>()
                    });
                }

                _logger.LogInformation("[ObtenirAssociations] {Count} associations récupérées avec succès", associations.Count);
                
                return Ok(new { 
                    message = "Associations récupérées avec succès",
                    associations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ObtenirAssociations] Erreur lors de la récupération des associations");
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération des associations" });
            }
        }

        [HttpGet("ObtenirAssociation/{id}")]
        public async Task<IActionResult> ObtenirAssociationParId(int id)
        {
            try
            {
                _logger.LogInformation("[ObtenirAssociationParId] Récupération de l'association avec l'ID {Id}", id);

                // Vérifier si l'ID est valide
                if (id <= 0)
                {
                    _logger.LogWarning("[ObtenirAssociationParId] ID invalide fourni : {Id}", id);
                    return BadRequest(new { message = "L'ID de l'association doit être supérieur à 0" });
                }

                // Récupérer l'association avec ses membres et événements
                var association = await _context.Associations
                    .Include(a => a.Membres)
                    .Include(a => a.Evenements)
                    .Select(a => new
                    {
                        a.Id,
                        a.Nom,
                        a.Description,
                        a.Categorie,
                        a.Faculte,
                        a.CodePermanentCreateur,
                        a.DateCreation,
                        a.Statut,
                        Objectifs = a.Objectifs ?? new List<string>(),
                        Activites = a.Activites ?? new List<string>(),
                        Membres = a.Membres.Select(m => new
                        {
                            m.Id,
                            m.CodePermanent,
                            m.Role,
                            m.DateAdhesion,
                            m.Statut,
                            Responsabilites = m.Responsabilites ?? new List<string>()
                        }).ToList(),
                        Evenements = a.Evenements.Select(e => new
                        {
                            e.Id,
                            e.Titre,
                            e.Description,
                            e.DateDebut,
                            e.DateFin,
                            e.Lieu,
                            e.CapaciteMax,
                            Participants = e.Participants ?? new List<string>(),
                            e.Statut
                        }).ToList()
                    })
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (association == null)
                {
                    _logger.LogWarning("[ObtenirAssociationParId] Association non trouvée avec l'ID : {Id}", id);
                    return NotFound(new { message = $"Aucune association trouvée avec l'ID {id}" });
                }

                _logger.LogInformation("[ObtenirAssociationParId] Association {Id} récupérée avec succès", id);
                
                return Ok(new { 
                    message = "Association récupérée avec succès",
                    association
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ObtenirAssociationParId] Erreur lors de la récupération de l'association {Id}", id);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération de l'association" });
            }
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

                return CreatedAtAction(nameof(ObtenirAssociationParId), new { id = association.Id }, association);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de l'association");
                return StatusCode(500, "Une erreur est survenue lors de la création de l'association");
            }
        }

        [HttpPut("ModifierAssociation/{id}")]
        public async Task<IActionResult> ModifierAssociation(int id, [FromBody] ModificationAssociation modif)
        {
            try
            {
                _logger.LogInformation("[ModifierAssociation] Tentative de modification de l'association {Id}", id);

                // Validation de base
                if (modif == null)
                {
                    return BadRequest("Les données de modification ne peuvent pas être nulles");
                }

                if (id != modif.Id)
                {
                    return BadRequest("L'ID de l'association ne correspond pas à l'ID dans l'URL");
                }

                // Récupérer l'association existante avec ses membres
                var associationExistante = await _context.Associations
                    .Include(a => a.Membres)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (associationExistante == null)
                {
                    _logger.LogWarning("[ModifierAssociation] Association {Id} non trouvée", id);
                    return NotFound($"Association avec l'ID {id} non trouvée");
                }

                // Vérifier si l'utilisateur existe et est connecté
                var client = _httpClientFactory.CreateClient("AuthService");
                var response = await client.GetAsync($"api/Authentification/VerifierUtilisateur/{modif.CodePermanentCreateur}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("[ModifierAssociation] Utilisateur non autorisé : {CodePermanent}, Status: {Status}, Response: {Response}",
                        modif.CodePermanentCreateur, response.StatusCode, errorContent);
                    return BadRequest("L'utilisateur doit être connecté pour modifier une association");
                }

                // Désérialiser la réponse pour obtenir le rôle de l'utilisateur
                var userInfo = await JsonSerializer.DeserializeAsync<JsonElement>(await response.Content.ReadAsStreamAsync());
                var roleUtilisateur = userInfo.GetProperty("role").GetInt32();

                // Vérifier les droits d'accès
                var membreModificateur = associationExistante.Membres
                    .FirstOrDefault(m => m.CodePermanent == modif.CodePermanentCreateur);

                bool peutModifier = false;

                // Admin peut tout modifier
                if (roleUtilisateur == ROLE_ADMIN)
                {
                    peutModifier = true;
                }
                // Vérifier si l'utilisateur est président ou vice-président de l'association
                else if (membreModificateur != null && 
                    (membreModificateur.Role == RoleMembre.President || 
                     membreModificateur.Role == RoleMembre.VicePresident))
                {
                    peutModifier = true;
                }

                if (!peutModifier)
                {
                    _logger.LogWarning("[ModifierAssociation] Accès refusé pour l'utilisateur {CodePermanent}", modif.CodePermanentCreateur);
                    return Forbid("Vous n'avez pas les droits nécessaires pour modifier cette association");
                }

                // Vérifier si le nouveau nom n'est pas déjà utilisé (sauf si c'est le même que l'actuel)
                if (modif.Nom != associationExistante.Nom)
                {
                    var nomExiste = await _context.Associations
                        .AnyAsync(a => a.Nom.ToLower() == modif.Nom.ToLower() && a.Id != id);

                    if (nomExiste)
                    {
                        return BadRequest("Une association avec ce nom existe déjà");
                    }
                }

                // Mettre à jour les propriétés modifiables
                associationExistante.Nom = modif.Nom;
                associationExistante.Description = modif.Description;
                associationExistante.Categorie = modif.Categorie;
                associationExistante.Faculte = modif.Faculte;
                
                // Mettre à jour les listes seulement si elles sont fournies
                if (modif.Objectifs != null)
                {
                    associationExistante.Objectifs = modif.Objectifs;
                }
                if (modif.Activites != null)
                {
                    associationExistante.Activites = modif.Activites;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("[ModifierAssociation] Association {Id} modifiée avec succès", id);

                return Ok(new { 
                    message = "Association modifiée avec succès",
                    association = associationExistante 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ModifierAssociation] Erreur lors de la modification de l'association {Id}", id);
                return StatusCode(500, "Une erreur est survenue lors de la modification de l'association");
            }
        }

        [HttpDelete("SupprimerAssociation/{id}")]
        public async Task<IActionResult> SupprimerAssociation(int id, [FromQuery] string codePermanentUtilisateur)
        {
            try
            {
                _logger.LogInformation("[SupprimerAssociation] Tentative de suppression de l'association {Id} par l'utilisateur {CodePermanent}", id, codePermanentUtilisateur);

                // Validation de base
                if (id <= 0)
                {
                    _logger.LogWarning("[SupprimerAssociation] ID invalide fourni : {Id}", id);
                    return BadRequest(new { message = "L'ID de l'association doit être supérieur à 0" });
                }

                if (string.IsNullOrWhiteSpace(codePermanentUtilisateur))
                {
                    _logger.LogWarning("[SupprimerAssociation] Code permanent de l'utilisateur non fourni");
                    return BadRequest(new { message = "Le code permanent de l'utilisateur est requis" });
                }

                // Récupérer l'association avec ses membres et événements
                var association = await _context.Associations
                    .Include(a => a.Membres)
                    .Include(a => a.Evenements)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (association == null)
                {
                    _logger.LogWarning("[SupprimerAssociation] Association {Id} non trouvée", id);
                    return NotFound(new { message = $"Association avec l'ID {id} non trouvée" });
                }

                // Vérifier si l'utilisateur existe et est connecté
                var client = _httpClientFactory.CreateClient("AuthService");
                var response = await client.GetAsync($"api/Authentification/VerifierUtilisateur/{codePermanentUtilisateur}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("[SupprimerAssociation] Utilisateur non autorisé : {CodePermanent}, Status: {Status}, Response: {Response}",
                        codePermanentUtilisateur, response.StatusCode, errorContent);
                    return BadRequest(new { message = "L'utilisateur doit être connecté pour supprimer une association" });
                }

                // Désérialiser la réponse pour obtenir les informations de l'utilisateur
                var userInfo = await JsonSerializer.DeserializeAsync<JsonElement>(await response.Content.ReadAsStreamAsync());
                
                // Vérifier si l'utilisateur existe
                if (!userInfo.GetProperty("existe").GetBoolean())
                {
                    _logger.LogWarning("[SupprimerAssociation] Utilisateur inexistant : {CodePermanent}", codePermanentUtilisateur);
                    return BadRequest(new { message = "L'utilisateur spécifié n'existe pas" });
                }

                // Vérifier si l'utilisateur est connecté
                if (!userInfo.GetProperty("estConnecte").GetBoolean())
                {
                    _logger.LogWarning("[SupprimerAssociation] Utilisateur non connecté : {CodePermanent}", codePermanentUtilisateur);
                    return BadRequest(new { 
                        message = "L'utilisateur doit être connecté pour supprimer une association",
                        derniereConnexion = userInfo.GetProperty("derniereConnexion").GetString()
                    });
                }

                // Récupérer le rôle de l'utilisateur
                var roleUtilisateur = userInfo.GetProperty("role").GetInt32();

                // Vérifier les droits d'accès
                var membreDemandeur = association.Membres
                    .FirstOrDefault(m => m.CodePermanent == codePermanentUtilisateur);

                bool peutSupprimer = false;

                // Admin peut tout supprimer
                if (roleUtilisateur == ROLE_ADMIN)
                {
                    peutSupprimer = true;
                }
                // Vérifier si l'utilisateur est le créateur de l'association
                else if (association.CodePermanentCreateur == codePermanentUtilisateur)
                {
                    peutSupprimer = true;
                }
                // Vérifier si l'utilisateur est président de l'association
                else if (membreDemandeur != null && membreDemandeur.Role == RoleMembre.President)
                {
                    peutSupprimer = true;
                }

                if (!peutSupprimer)
                {
                    _logger.LogWarning("[SupprimerAssociation] Accès refusé pour l'utilisateur {CodePermanent}", codePermanentUtilisateur);
                    return StatusCode(403, new { message = "Vous n'avez pas les droits nécessaires pour supprimer cette association" });
                }

                // Vérifier s'il y a des événements en cours ou à venir
                var evenementsEnCours = association.Evenements
                    .Where(e => e.DateDebut <= DateTime.UtcNow && e.DateFin >= DateTime.UtcNow)
                    .ToList();

                if (evenementsEnCours.Any())
                {
                    _logger.LogWarning("[SupprimerAssociation] Impossible de supprimer l'association {Id} : événements en cours", id);
                    return BadRequest(new { 
                        message = "Impossible de supprimer l'association car elle a des événements en cours",
                        evenementsEnCours = evenementsEnCours.Select(e => new { e.Id, e.Titre, e.DateDebut, e.DateFin })
                    });
                }

                // Supprimer l'association (Entity Framework supprimera automatiquement les membres et événements associés)
                _context.Associations.Remove(association);
                await _context.SaveChangesAsync();

                _logger.LogInformation("[SupprimerAssociation] Association {Id} supprimée avec succès par l'utilisateur {CodePermanent}", id, codePermanentUtilisateur);

                return Ok(new { 
                    message = "Association supprimée avec succès",
                    associationSupprimee = new
                    {
                        association.Id,
                        association.Nom,
                        association.CodePermanentCreateur,
                        nombreMembres = association.Membres.Count,
                        nombreEvenements = association.Evenements.Count
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SupprimerAssociation] Erreur lors de la suppression de l'association {Id}", id);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la suppression de l'association" });
            }
        }

        // Gestion des membres
        [HttpGet("ObtenirMembres/{id}")]
        public async Task<IActionResult> ObtenirMembres(int id)
        {
            try
            {
                _logger.LogInformation("[ObtenirMembres] Récupération des membres pour l'association {Id}", id);

                // Vérifier si l'ID est valide
                if (id <= 0)
                {
                    _logger.LogWarning("[ObtenirMembres] ID invalide fourni : {Id}", id);
                    return BadRequest(new { message = "L'ID de l'association doit être supérieur à 0" });
                }

                // Récupérer l'association avec ses membres
                var association = await _context.Associations
                    .Include(a => a.Membres)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (association == null)
                {
                    _logger.LogWarning("[ObtenirMembres] Association {Id} non trouvée", id);
                    return NotFound(new { message = $"Association avec l'ID {id} non trouvée" });
                }

                // Préparer la liste des membres avec des informations détaillées
                var membres = new List<object>();
                
                if (association.Membres != null)
                {
                    membres = association.Membres
                        .OrderBy(m => m.Role) // Trier par rôle (Président en premier, puis Vice-président, etc.)
                        .ThenBy(m => m.DateAdhesion) // Puis par date d'adhésion
                        .Select(m => new
                        {
                            m.Id,
                            m.CodePermanent,
                            m.Role,
                            m.DateAdhesion,
                            m.Statut,
                            Responsabilites = m.Responsabilites ?? new List<string>(),
                            Associations = m.Associations ?? new List<string>(),
                            m.RoleUtilisateur,
                            // Calculer l'ancienneté en jours
                            AncienneteJours = (DateTime.UtcNow - m.DateAdhesion).Days,
                            // Formater la date d'adhésion
                            DateAdhesionFormatee = m.DateAdhesion.ToString("dd/MM/yyyy")
                        })
                        .Cast<object>()
                        .ToList();
                }

                _logger.LogInformation("[ObtenirMembres] {Count} membres récupérés pour l'association {Id}", membres.Count, id);

                return Ok(new { 
                    message = "Membres récupérés avec succès",
                    association = new
                    {
                        association.Id,
                        association.Nom,
                        association.CodePermanentCreateur
                    },
                    membres,
                    totalMembres = membres.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ObtenirMembres] Erreur lors de la récupération des membres pour l'association {Id}", id);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération des membres" });
            }
        }

        [HttpPost("DemanderAdhesion/{id}")]
        public async Task<IActionResult> DemanderAdhesion(int id, [FromBody] DemandeAdhesion demande)
        {
            try
            {
                _logger.LogInformation("[DemanderAdhesion] Demande d'adhésion pour l'association {Id} par {CodePermanent}", id, demande?.CodePermanent);

                if (demande == null)
                {
                    return BadRequest(new { message = "La demande d'adhésion ne peut pas être nulle" });
                }

                // Validation des champs obligatoires
                if (string.IsNullOrWhiteSpace(demande.CodePermanent))
                {
                    return BadRequest(new { message = "Le code permanent est obligatoire" });
                }

                if (string.IsNullOrWhiteSpace(demande.Motivation))
                {
                    return BadRequest(new { message = "La motivation est obligatoire" });
                }

                // Vérifier si l'association existe
                var association = await _context.Associations
                    .Include(a => a.Membres)
                    .Include(a => a.DemandesAdhesion)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (association == null)
                {
                    return NotFound(new { message = "Association non trouvée" });
                }

                // Vérifier si l'utilisateur existe (appel au service d'authentification)
                var client = _httpClientFactory.CreateClient("AuthService");
                var response = await client.GetAsync($"api/Authentification/VerifierUtilisateur/{demande.CodePermanent}");

                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest(new { message = "Utilisateur non trouvé ou non connecté" });
                }

                // Vérifier si l'utilisateur n'est pas déjà membre
                if (association.Membres.Any(m => m.CodePermanent == demande.CodePermanent))
                {
                    return BadRequest(new { message = "Vous êtes déjà membre de cette association" });
                }

                // Vérifier s'il n'y a pas déjà une demande en cours
                if (association.DemandesAdhesion?.Any(d => d.CodePermanent == demande.CodePermanent && d.Statut == StatutDemandeAdhesion.EnAttente) == true)
                {
                    return BadRequest(new { message = "Vous avez déjà une demande d'adhésion en cours pour cette association" });
                }

                // Créer la nouvelle demande
                var nouvelleDemande = new DemandeAdhesion
                {
                    CodePermanent = demande.CodePermanent,
                    Role = demande.Role,
                    DateDemande = DateTime.UtcNow,
                    Motivation = demande.Motivation,
                    Statut = StatutDemandeAdhesion.EnAttente
                };

                // Initialiser la liste si elle est null
                if (association.DemandesAdhesion == null)
                {
                    association.DemandesAdhesion = new List<DemandeAdhesion>();
                }

                association.DemandesAdhesion.Add(nouvelleDemande);
                await _context.SaveChangesAsync();

                _logger.LogInformation("[DemanderAdhesion] Demande d'adhésion créée avec succès pour l'association {Id}", id);

                return Ok(new { 
                    message = "Demande d'adhésion créée avec succès. Elle sera examinée par le président ou vice-président de l'association.",
                    demande = new
                    {
                        nouvelleDemande.Id,
                        nouvelleDemande.CodePermanent,
                        nouvelleDemande.Role,
                        nouvelleDemande.DateDemande,
                        nouvelleDemande.Motivation,
                        nouvelleDemande.Statut
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DemanderAdhesion] Erreur lors de la création de la demande d'adhésion pour l'association {Id}", id);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la création de la demande d'adhésion" });
            }
        }

        [HttpPost("AjouterMembre/{id}")]
        public async Task<IActionResult> AjouterMembre(int id, [FromBody] RepondreDemandeRequest request)
        {
            try
            {
                _logger.LogInformation("[AjouterMembre] Réponse à la demande d'adhésion {DemandeId} pour l'association {Id}", request?.DemandeId, id);

                if (request == null)
                {
                    return BadRequest(new { message = "La requête ne peut pas être nulle" });
                }

                if (string.IsNullOrWhiteSpace(request.CodePermanentRepondant))
                {
                    return BadRequest(new { message = "Le code permanent de l'utilisateur répondant est obligatoire" });
                }

                // Récupérer l'association avec ses membres et demandes
                var association = await _context.Associations
                    .Include(a => a.Membres)
                    .Include(a => a.DemandesAdhesion)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (association == null)
                {
                    return NotFound(new { message = "Association non trouvée" });
                }

                // Vérifier si l'utilisateur répondant est président ou vice-président
                var membreRepondant = association.Membres
                    .FirstOrDefault(m => m.CodePermanent == request.CodePermanentRepondant);

                if (membreRepondant == null || (membreRepondant.Role != RoleMembre.President && membreRepondant.Role != RoleMembre.VicePresident))
                {
                    return Unauthorized(new { message = "Seuls le président et le vice-président peuvent gérer les demandes d'adhésion" });
                }

                // Trouver la demande d'adhésion
                var demande = association.DemandesAdhesion?
                    .FirstOrDefault(d => d.Id == request.DemandeId && d.Statut == StatutDemandeAdhesion.EnAttente);

                if (demande == null)
                {
                    return NotFound(new { message = "Demande d'adhésion non trouvée ou déjà traitée" });
                }

                if (request.Accepter)
                {
                    // Vérifier qu'il n'y a pas déjà un membre avec le rôle demandé (pour les rôles de direction)
                    if (demande.Role == RoleMembre.President || 
                        demande.Role == RoleMembre.VicePresident || 
                        demande.Role == RoleMembre.Tresorier)
                    {
                        var membreExistant = association.Membres
                            .FirstOrDefault(m => m.Role == demande.Role);

                        if (membreExistant != null)
                        {
                            _logger.LogWarning("[AjouterMembre] Tentative d'ajout d'un doublon pour le rôle {Role} dans l'association {Id}", demande.Role, id);
                            return BadRequest(new { 
                                message = $"Il y a déjà un {demande.Role} dans cette association. Un seul membre peut avoir ce rôle.",
                                membreExistant = new
                                {
                                    codePermanent = membreExistant.CodePermanent,
                                    role = membreExistant.Role.ToString(),
                                    dateAdhesion = membreExistant.DateAdhesion
                                },
                                suggestion = "Modifiez d'abord le rôle du membre existant ou refusez cette demande"
                            });
                        }
                    }

                    // Créer le nouveau membre
                    var nouveauMembre = new Membre
                    {
                        CodePermanent = demande.CodePermanent,
                        Role = demande.Role,
                        DateAdhesion = DateTime.UtcNow,
                        Statut = StatutMembre.Actif,
                        Responsabilites = new List<string>(),
                        Associations = new List<string>()
                    };

                    association.Membres.Add(nouveauMembre);
                    _logger.LogInformation("[AjouterMembre] Membre {CodePermanent} ajouté à l'association {Id}", demande.CodePermanent, id);
                }

                // Supprimer la demande de la liste
                association.DemandesAdhesion.Remove(demande);

                await _context.SaveChangesAsync();

                var message = request.Accepter 
                    ? "Demande d'adhésion acceptée avec succès. Le membre a été ajouté à l'association."
                    : "Demande d'adhésion refusée avec succès.";

                _logger.LogInformation("[AjouterMembre] Demande {DemandeId} {Action} par {CodePermanent}", 
                    request.DemandeId, request.Accepter ? "acceptée" : "refusée", request.CodePermanentRepondant);

                return Ok(new { 
                    message = message,
                    demande = new
                    {
                        demande.Id,
                        demande.CodePermanent,
                        demande.Role,
                        demande.DateDemande,
                        demande.Motivation,
                        Statut = request.Accepter ? StatutDemandeAdhesion.Acceptee : StatutDemandeAdhesion.Refusee
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AjouterMembre] Erreur lors de la réponse à la demande d'adhésion pour l'association {Id}", id);
                return StatusCode(500, new { message = "Une erreur est survenue lors du traitement de la demande d'adhésion" });
            }
        }

        
        [HttpPut("ModifierRoleMembre/{id}/{codePermanent}")]
        public async Task<IActionResult> ModifierRoleMembre(int id, string codePermanent, [FromBody] ModifierRoleRequest request)
        {
            try
            {
                _logger.LogInformation("[ModifierRoleMembre] Tentative de modification du rôle du membre {CodePermanent} dans l'association {Id} par {CodePermanentModifiant}", 
                    codePermanent, id, request?.CodePermanentModifiant);

                // Validation des paramètres
                if (id <= 0)
                {
                    _logger.LogWarning("[ModifierRoleMembre] ID invalide fourni : {Id}", id);
                    return BadRequest(new { message = "L'ID de l'association doit être supérieur à 0" });
                }

                if (string.IsNullOrWhiteSpace(codePermanent))
                {
                    return BadRequest(new { message = "Le code permanent du membre est obligatoire" });
                }

                if (request == null)
                {
                    return BadRequest(new { message = "La requête ne peut pas être nulle" });
                }

                if (string.IsNullOrWhiteSpace(request.CodePermanentModifiant))
                {
                    return BadRequest(new { message = "Le code permanent de l'utilisateur qui modifie le rôle est obligatoire" });
                }

                // Vérifier que l'utilisateur ne modifie pas son propre rôle
                if (codePermanent == request.CodePermanentModifiant)
                {
                    _logger.LogWarning("[ModifierRoleMembre] Tentative de modification de son propre rôle : {CodePermanent}", codePermanent);
                    return BadRequest(new { message = "Un membre ne peut pas modifier son propre rôle" });
                }

                // Récupérer l'association avec ses membres
                var association = await _context.Associations
                    .Include(a => a.Membres)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (association == null)
                {
                    _logger.LogWarning("[ModifierRoleMembre] Association {Id} non trouvée", id);
                    return NotFound(new { message = "Association non trouvée" });
                }

                // Vérifier si l'utilisateur qui modifie est autorisé (Président ou Vice-président)
                var membreModifiant = association.Membres
                    .FirstOrDefault(m => m.CodePermanent == request.CodePermanentModifiant);

                if (membreModifiant == null)
                {
                    _logger.LogWarning("[ModifierRoleMembre] Utilisateur {CodePermanentModifiant} n'est pas membre de l'association {Id}", request.CodePermanentModifiant, id);
                    return Unauthorized(new { message = "Vous devez être membre de cette association pour modifier les rôles" });
                }

                if (membreModifiant.Role != RoleMembre.President && membreModifiant.Role != RoleMembre.VicePresident)
                {
                    _logger.LogWarning("[ModifierRoleMembre] Utilisateur {CodePermanentModifiant} avec rôle {Role} non autorisé pour l'association {Id}", 
                        request.CodePermanentModifiant, membreModifiant.Role, id);
                    return Unauthorized(new { 
                        message = "Seuls le président et le vice-président peuvent modifier les rôles",
                        roleActuel = membreModifiant.Role.ToString(),
                        rolesAutorises = new[] { "President", "VicePresident" }
                    });
                }

                // Trouver le membre dont on veut modifier le rôle
                var membreAModifier = association.Membres
                    .FirstOrDefault(m => m.CodePermanent == codePermanent);

                if (membreAModifier == null)
                {
                    _logger.LogWarning("[ModifierRoleMembre] Membre {CodePermanent} non trouvé dans l'association {Id}", codePermanent, id);
                    return NotFound(new { message = "Membre non trouvé dans cette association" });
                }

                // Vérifier que le nouveau rôle est différent de l'actuel
                if (membreAModifier.Role == request.NouveauRole)
                {
                    _logger.LogWarning("[ModifierRoleMembre] Tentative de modification vers le même rôle : {Role}", request.NouveauRole);
                    return BadRequest(new { message = "Le nouveau rôle doit être différent du rôle actuel" });
                }

                // Vérifier que le membre à modifier n'est pas le créateur de l'association
                if (membreAModifier.CodePermanent == association.CodePermanentCreateur)
                {
                    _logger.LogWarning("[ModifierRoleMembre] Tentative de modification du rôle du créateur : {CodePermanent}", codePermanent);
                    return BadRequest(new { message = "Le rôle du créateur de l'association ne peut pas être modifié" });
                }

                // Règles spéciales pour la modification des rôles
                var ancienRole = membreAModifier.Role;
                
                // Seul le président peut promouvoir quelqu'un au rôle de président
                if (request.NouveauRole == RoleMembre.President && membreModifiant.Role != RoleMembre.President)
                {
                    _logger.LogWarning("[ModifierRoleMembre] Tentative de promotion au rôle président par un non-président");
                    return BadRequest(new { message = "Seul le président peut promouvoir quelqu'un au rôle de président" });
                }

                // Un vice-président ne peut pas promouvoir quelqu'un au rôle de président
                if (request.NouveauRole == RoleMembre.President && membreModifiant.Role == RoleMembre.VicePresident)
                {
                    _logger.LogWarning("[ModifierRoleMembre] Tentative de promotion au rôle président par un vice-président");
                    return BadRequest(new { message = "Un vice-président ne peut pas promouvoir quelqu'un au rôle de président" });
                }

                // Vérifier qu'il n'y a pas déjà un membre avec le nouveau rôle de direction
                if (request.NouveauRole == RoleMembre.President || 
                    request.NouveauRole == RoleMembre.VicePresident || 
                    request.NouveauRole == RoleMembre.Tresorier)
                {
                    var membreExistant = association.Membres
                        .FirstOrDefault(m => m.CodePermanent != codePermanent && m.Role == request.NouveauRole);

                    if (membreExistant != null)
                    {
                        _logger.LogWarning("[ModifierRoleMembre] Tentative de création d'un doublon pour le rôle {Role} dans l'association {Id}", request.NouveauRole, id);
                        return BadRequest(new { 
                            message = $"Il y a déjà un {request.NouveauRole} dans cette association. Un seul membre peut avoir ce rôle.",
                            membreExistant = new
                            {
                                codePermanent = membreExistant.CodePermanent,
                                role = membreExistant.Role.ToString(),
                                dateAdhesion = membreExistant.DateAdhesion
                            }
                        });
                    }
                }

                // Sauvegarder l'ancien rôle pour les logs
                var ancienRoleInfo = new
                {
                    membreAModifier.Id,
                    membreAModifier.CodePermanent,
                    ancienRole = membreAModifier.Role,
                    membreAModifier.DateAdhesion,
                    membreAModifier.Statut,
                    Responsabilites = membreAModifier.Responsabilites ?? new List<string>(),
                    Associations = membreAModifier.Associations ?? new List<string>()
                };

                // Modifier le rôle
                membreAModifier.Role = request.NouveauRole;
                await _context.SaveChangesAsync();

                _logger.LogInformation("[ModifierRoleMembre] Rôle du membre {CodePermanent} modifié de {AncienRole} vers {NouveauRole} dans l'association {Id} par {CodePermanentModifiant}", 
                    codePermanent, ancienRole, request.NouveauRole, id, request.CodePermanentModifiant);

                return Ok(new { 
                    message = "Rôle du membre modifié avec succès",
                    membreModifie = new
                    {
                        membreAModifier.Id,
                        membreAModifier.CodePermanent,
                        ancienRole = ancienRole,
                        nouveauRole = membreAModifier.Role,
                        membreAModifier.DateAdhesion,
                        membreAModifier.Statut,
                        Responsabilites = membreAModifier.Responsabilites ?? new List<string>(),
                        Associations = membreAModifier.Associations ?? new List<string>()
                    },
                    association = new
                    {
                        association.Id,
                        association.Nom
                    },
                    actionEffectueePar = new
                    {
                        codePermanent = membreModifiant.CodePermanent,
                        role = membreModifiant.Role.ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ModifierRoleMembre] Erreur lors de la modification du rôle du membre {CodePermanent} dans l'association {Id}", codePermanent, id);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la modification du rôle" });
            }
        }

        [HttpDelete("RetirerMembre/{id}/{codePermanent}")]
        public async Task<IActionResult> RetirerMembre(int id, string codePermanent, [FromQuery] string codePermanentRetirant)
        {
            try
            {
                _logger.LogInformation("[RetirerMembre] Tentative de retrait du membre {CodePermanent} de l'association {Id} par {CodePermanentRetirant}", 
                    codePermanent, id, codePermanentRetirant);

                // Validation des paramètres
                if (id <= 0)
                {
                    _logger.LogWarning("[RetirerMembre] ID invalide fourni : {Id}", id);
                    return BadRequest(new { message = "L'ID de l'association doit être supérieur à 0" });
                }

                if (string.IsNullOrWhiteSpace(codePermanent))
                {
                    return BadRequest(new { message = "Le code permanent du membre à retirer est obligatoire" });
                }

                if (string.IsNullOrWhiteSpace(codePermanentRetirant))
                {
                    return BadRequest(new { message = "Le code permanent de l'utilisateur qui retire le membre est obligatoire" });
                }

                // Vérifier que l'utilisateur ne se retire pas lui-même
                if (codePermanent == codePermanentRetirant)
                {
                    _logger.LogWarning("[RetirerMembre] Tentative de retrait de soi-même : {CodePermanent}", codePermanent);
                    return BadRequest(new { message = "Un membre ne peut pas se retirer lui-même de l'association" });
                }

                // Récupérer l'association avec ses membres
                var association = await _context.Associations
                    .Include(a => a.Membres)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (association == null)
                {
                    _logger.LogWarning("[RetirerMembre] Association {Id} non trouvée", id);
                    return NotFound(new { message = "Association non trouvée" });
                }

                // Vérifier si l'utilisateur qui retire est autorisé (Président, Vice-président, Trésorier, Secrétaire)
                var membreRetirant = association.Membres
                    .FirstOrDefault(m => m.CodePermanent == codePermanentRetirant);

                if (membreRetirant == null)
                {
                    _logger.LogWarning("[RetirerMembre] Utilisateur {CodePermanentRetirant} n'est pas membre de l'association {Id}", codePermanentRetirant, id);
                    return Unauthorized(new { message = "Vous devez être membre de cette association pour retirer un membre" });
                }

                if (membreRetirant.Role != RoleMembre.President && 
                    membreRetirant.Role != RoleMembre.VicePresident && 
                    membreRetirant.Role != RoleMembre.Tresorier && 
                    membreRetirant.Role != RoleMembre.Secretaire)
                {
                    _logger.LogWarning("[RetirerMembre] Utilisateur {CodePermanentRetirant} avec rôle {Role} non autorisé pour l'association {Id}", 
                        codePermanentRetirant, membreRetirant.Role, id);
                    return Unauthorized(new { 
                        message = "Seuls le président, vice-président, trésorier et secrétaire peuvent retirer des membres",
                        roleActuel = membreRetirant.Role.ToString(),
                        rolesAutorises = new[] { "President", "VicePresident", "Tresorier", "Secretaire" }
                    });
                }

                // Trouver le membre à retirer
                var membreARetirer = association.Membres
                    .FirstOrDefault(m => m.CodePermanent == codePermanent);

                if (membreARetirer == null)
                {
                    _logger.LogWarning("[RetirerMembre] Membre {CodePermanent} non trouvé dans l'association {Id}", codePermanent, id);
                    return NotFound(new { message = "Membre non trouvé dans cette association" });
                }

                // Vérifier que le membre à retirer n'est pas le créateur de l'association
                if (membreARetirer.CodePermanent == association.CodePermanentCreateur)
                {
                    _logger.LogWarning("[RetirerMembre] Tentative de retrait du créateur de l'association : {CodePermanent}", codePermanent);
                    return BadRequest(new { message = "Le créateur de l'association ne peut pas être retiré" });
                }

                // Vérifier que le membre à retirer n'est pas président (sauf si c'est le créateur qui le retire)
                if (membreARetirer.Role == RoleMembre.President && membreRetirant.Role != RoleMembre.President)
                {
                    _logger.LogWarning("[RetirerMembre] Tentative de retrait du président par un non-président : {CodePermanent}", codePermanent);
                    return BadRequest(new { message = "Seul le président peut retirer un autre président" });
                }

                // Vérifier qu'il ne reste pas qu'un seul membre après le retrait
                if (association.Membres.Count == 1)
                {
                    _logger.LogWarning("[RetirerMembre] Tentative de retrait du dernier membre de l'association {Id}", id);
                    return BadRequest(new { 
                        message = "Impossible de retirer le dernier membre de l'association. Une association doit avoir au moins un membre.",
                        suggestion = "Considérez plutôt supprimer l'association si elle n'a plus d'activité"
                    });
                }

                // Sauvegarder les informations du membre avant suppression
                var membreSupprime = new
                {
                    membreARetirer.Id,
                    membreARetirer.CodePermanent,
                    membreARetirer.Role,
                    membreARetirer.DateAdhesion,
                    membreARetirer.Statut,
                    Responsabilites = membreARetirer.Responsabilites ?? new List<string>(),
                    Associations = membreARetirer.Associations ?? new List<string>()
                };

                // Retirer le membre de l'association
                association.Membres.Remove(membreARetirer);
                await _context.SaveChangesAsync();

                _logger.LogInformation("[RetirerMembre] Membre {CodePermanent} retiré de l'association {Id} par {CodePermanentRetirant}", 
                    codePermanent, id, codePermanentRetirant);

                return Ok(new { 
                    message = "Membre retiré avec succès de l'association",
                    membreRetire = membreSupprime,
                    association = new
                    {
                        association.Id,
                        association.Nom,
                        nombreMembresRestants = association.Membres.Count
                    },
                    actionEffectueePar = new
                    {
                        codePermanent = membreRetirant.CodePermanent,
                        role = membreRetirant.Role.ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RetirerMembre] Erreur lors du retrait du membre {CodePermanent} de l'association {Id}", codePermanent, id);
                return StatusCode(500, new { message = "Une erreur est survenue lors du retrait du membre" });
            }
        }

        // Gestion des événements
        
        [HttpGet("ObtenirEvenements/{id}")]
        public async Task<IActionResult> ObtenirEvenements(int id, [FromQuery] string? codePermanentUtilisateur = null)
        {
            try
            {
                _logger.LogInformation("[ObtenirEvenements] Récupération des événements pour l'association {Id}", id);

                // 1. Validation de l'association
                var association = await _context.Associations
                    .Include(a => a.Evenements)
                    .Include(a => a.Membres)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (association == null)
                {
                    _logger.LogWarning("[ObtenirEvenements] Association {Id} non trouvée", id);
                    return NotFound(new { 
                        message = "Association non trouvée",
                        associationId = id
                    });
                }

                // 2. Récupération des événements avec tri par date de début
                var evenements = association.Evenements?
                    .OrderBy(e => e.DateDebut)
                    .ToList() ?? new List<Evenement>();

                // 3. Préparation de la réponse
                var evenementsDetails = evenements.Select(e => new
                {
                    e.Id,
                    e.Titre,
                    e.Description,
                    e.DateDebut,
                    e.DateFin,
                    e.Lieu,
                    e.CapaciteMax,
                    e.Statut,
                    nombreParticipants = e.Participants?.Count ?? 0
                }).ToList();

                _logger.LogInformation("[ObtenirEvenements] {NombreEvenements} événements récupérés pour l'association {Id}", 
                    evenementsDetails.Count, id);

                return Ok(new
                {
                    message = "Événements récupérés avec succès",
                    association = new
                    {
                        association.Id,
                        association.Nom,
                        association.Statut
                    },
                    evenements = evenementsDetails,
                    totalEvenements = evenementsDetails.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ObtenirEvenements] Erreur lors de la récupération des événements pour l'association {Id}", id);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération des événements" });
            }
        }

        [HttpPost("CreerEvenement/{id}")]
        public async Task<IActionResult> CreerEvenement(int id, [FromBody] Evenement evenement, [FromQuery] string codePermanentUtilisateur)
        {
            try
            {
                _logger.LogInformation("[CreerEvenement] Création d'un événement pour l'association {AssociationId} par {CodePermanent}", id, codePermanentUtilisateur);

                // 1. Validation des paramètres
                if (evenement == null)
                {
                    _logger.LogWarning("[CreerEvenement] Objet événement null fourni");
                    return BadRequest(new { message = "Les données de l'événement sont obligatoires" });
                }

                if (string.IsNullOrWhiteSpace(codePermanentUtilisateur))
                {
                    _logger.LogWarning("[CreerEvenement] Code permanent utilisateur non fourni");
                    return BadRequest(new { message = "Le code permanent de l'utilisateur est obligatoire" });
                }

                // 2. Validation de l'association
                var association = await _context.Associations
                    .Include(a => a.Membres)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (association == null)
                {
                    _logger.LogWarning("[CreerEvenement] Association {AssociationId} non trouvée", id);
                    return NotFound(new { message = "Association non trouvée" });
                }

                // 3. Vérification des droits de l'utilisateur
                var membre = association.Membres
                    .FirstOrDefault(m => m.CodePermanent == codePermanentUtilisateur);

                if (membre == null)
                {
                    _logger.LogWarning("[CreerEvenement] Utilisateur {CodePermanent} n'est pas membre de l'association {AssociationId}", codePermanentUtilisateur, id);
                    return NotFound(new { 
                        message = "Vous n'êtes pas membre de cette association",
                        codePermanent = codePermanentUtilisateur,
                        associationId = id,
                        suggestion = "Demandez une adhésion à cette association"
                    });
                }

                // Vérification du statut du membre
                if (membre.Statut != StatutMembre.Actif)
                {
                    _logger.LogWarning("[CreerEvenement] Utilisateur {CodePermanent} n'est pas membre actif (statut: {Statut}) de l'association {AssociationId}", 
                        codePermanentUtilisateur, membre.Statut, id);
                    return Unauthorized(new { 
                        message = "vous devez actif c'est a dire connecter pour créer un événement",
                        statutActuel = membre.Statut.ToString(),
                        codePermanent = codePermanentUtilisateur,
                        associationId = id,
                        suggestion = "Contactez l'administration de l'association pour plus d'informations"
                    });
                }

                // 4. Vérification des permissions (seuls les rôles de direction peuvent créer)
                var rolesAutorises = new[] { RoleMembre.President, RoleMembre.VicePresident, RoleMembre.Tresorier, RoleMembre.Secretaire };
                if (!rolesAutorises.Contains(membre.Role))
                {
                    _logger.LogWarning("[CreerEvenement] Utilisateur {CodePermanent} n'a pas les droits pour créer un événement (rôle: {Role})", codePermanentUtilisateur, membre.Role);
                    return Unauthorized(new { 
                        message = "Seuls les membres President, VicePresident, Tresorier, Secretaire peuvent créer des événements",
                        roleActuel = membre.Role.ToString(),
                        rolesAutorises = rolesAutorises.Select(r => r.ToString()).ToArray()
                    });
                }

                // 5. Validation de l'événement
                if (string.IsNullOrWhiteSpace(evenement.Titre))
                {
                    _logger.LogWarning("[CreerEvenement] Titre de l'événement manquant");
                    return BadRequest(new { message = "Le titre de l'événement est obligatoire" });
                }

                if (string.IsNullOrWhiteSpace(evenement.Description))
                {
                    _logger.LogWarning("[CreerEvenement] Description de l'événement manquante");
                    return BadRequest(new { message = "La description de l'événement est obligatoire" });
                }

                if (evenement.DateDebut >= evenement.DateFin)
                {
                    _logger.LogWarning("[CreerEvenement] Dates invalides : début {DateDebut} >= fin {DateFin}", evenement.DateDebut, evenement.DateFin);
                    return BadRequest(new { message = "La date de fin doit être postérieure à la date de début" });
                }

                if (evenement.DateDebut <= DateTime.Now)
                {
                    _logger.LogWarning("[CreerEvenement] Date de début dans le passé : {DateDebut}", evenement.DateDebut);
                    return BadRequest(new { message = "La date de début doit être dans le futur" });
                }

                if (evenement.CapaciteMax.HasValue && evenement.CapaciteMax <= 0)
                {
                    _logger.LogWarning("[CreerEvenement] Capacité maximale invalide : {CapaciteMax}", evenement.CapaciteMax);
                    return BadRequest(new { message = "La capacité maximale doit être supérieure à 0" });
                }

                // 7. Configuration de l'événement
                evenement.Id = 0; // EF Core générera l'ID
                evenement.Statut = StatutEvenement.Planifie;
                evenement.Participants = new List<string>();

                // 8. Association de l'événement à l'association
                if (association.Evenements == null)
                    association.Evenements = new List<Evenement>();

                association.Evenements.Add(evenement);

                // 9. Sauvegarde
                await _context.SaveChangesAsync();

                _logger.LogInformation("[CreerEvenement] Événement créé avec succès pour l'association {AssociationId} par {CodePermanent}", id, codePermanentUtilisateur);

                return CreatedAtAction(nameof(ObtenirEvenements), new { id }, new
                {
                    message = "Événement créé avec succès",
                    evenement = new
                    {
                        evenement.Id,
                        evenement.Titre,
                        evenement.Description,
                        evenement.DateDebut,
                        evenement.DateFin,
                        evenement.Lieu,
                        evenement.CapaciteMax,
                        evenement.Statut,
                        nombreParticipants = evenement.Participants?.Count ?? 0
                    },
                    association = new
                    {
                        association.Id,
                        association.Nom,
                        nombreEvenements = association.Evenements.Count
                    },
                    creePar = new
                    {
                        codePermanent = membre.CodePermanent,
                        role = membre.Role.ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CreerEvenement] Erreur lors de la création de l'événement pour l'association {AssociationId}", id);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la création de l'événement" });
            }
        }

        [HttpPut("ModifierEvenement/{id}/{evenementId}")]
        public async Task<IActionResult> ModifierEvenement(int id, int evenementId, [FromBody] ModifierEvenementRequest evenementModifie, [FromQuery] string codePermanentUtilisateur)
        {
            try
            {
                _logger.LogInformation("[ModifierEvenement] Modification de l'événement {EvenementId} de l'association {AssociationId} par {CodePermanent}", 
                    evenementId, id, codePermanentUtilisateur);

                // 1. Validation des paramètres
                if (evenementModifie == null)
                {
                    _logger.LogWarning("[ModifierEvenement] Objet événement null fourni");
                    return BadRequest(new { message = "Les données de l'événement sont obligatoires" });
                }

                if (string.IsNullOrWhiteSpace(codePermanentUtilisateur))
                {
                    _logger.LogWarning("[ModifierEvenement] Code permanent utilisateur non fourni");
                    return BadRequest(new { message = "Le code permanent de l'utilisateur est obligatoire" });
                }

                // 2. Validation de l'association
                var association = await _context.Associations
                    .Include(a => a.Evenements)
                    .Include(a => a.Membres)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (association == null)
                {
                    _logger.LogWarning("[ModifierEvenement] Association {AssociationId} non trouvée", id);
                    return NotFound(new { message = "Association non trouvée" });
                }

                // 3. Validation de l'événement
                var evenementExistant = association.Evenements?
                    .FirstOrDefault(e => e.Id == evenementId);

                if (evenementExistant == null)
                {
                    _logger.LogWarning("[ModifierEvenement] Événement {EvenementId} non trouvé dans l'association {AssociationId}", evenementId, id);
                    return NotFound(new { 
                        message = "Événement non trouvé",
                        evenementId = evenementId,
                        associationId = id
                    });
                }

                // 4. Vérification du timing (événement pas encore commencé)
                if (evenementExistant.DateDebut <= DateTime.Now)
                {
                    _logger.LogWarning("[ModifierEvenement] Événement {EvenementId} a déjà commencé (début: {DateDebut})", 
                        evenementId, evenementExistant.DateDebut);
                    return BadRequest(new { 
                        message = "Impossible de modifier un événement qui a déjà commencé",
                        dateDebut = evenementExistant.DateDebut,
                        maintenant = DateTime.Now
                    });
                }

                // 5. Vérification des droits de l'utilisateur
                var membre = association.Membres?
                    .FirstOrDefault(m => m.CodePermanent == codePermanentUtilisateur);

                if (membre == null)
                {
                    _logger.LogWarning("[ModifierEvenement] Utilisateur {CodePermanent} n'est pas membre de l'association {AssociationId}", 
                        codePermanentUtilisateur, id);
                    return NotFound(new { 
                        message = "Vous n'êtes pas membre de cette association",
                        codePermanent = codePermanentUtilisateur,
                        associationId = id
                    });
                }

                // Vérification du statut du membre
                if (membre.Statut != StatutMembre.Actif)
                {
                    _logger.LogWarning("[ModifierEvenement] Utilisateur {CodePermanent} n'est pas membre actif (statut: {Statut})", 
                        codePermanentUtilisateur, membre.Statut);
                    return Unauthorized(new { 
                        message = "Vous devez être membre actif pour modifier un événement",
                        statutActuel = membre.Statut.ToString()
                    });
                }

                // 6. Vérification que l'utilisateur n'est pas participant
                if (evenementExistant.Participants?.Contains(codePermanentUtilisateur) == true)
                {
                    _logger.LogWarning("[ModifierEvenement] Utilisateur {CodePermanent} est participant de l'événement {EvenementId}", 
                        codePermanentUtilisateur, evenementId);
                    return Unauthorized(new { 
                        message = "Les participants ne peuvent pas modifier un événement",
                        codePermanent = codePermanentUtilisateur,
                        evenementId = evenementId
                    });
                }

                // 7. Validation des champs modifiables
                var modifications = new List<string>();

                // Validation de la date de début
                if (evenementModifie.DateDebut != evenementExistant.DateDebut)
                {
                    if (evenementModifie.DateDebut <= DateTime.Now)
                    {
                        _logger.LogWarning("[ModifierEvenement] Nouvelle date de début dans le passé : {DateDebut}", evenementModifie.DateDebut);
                        return BadRequest(new { message = "La nouvelle date de début doit être dans le futur" });
                    }
                    modifications.Add("Date de début");
                }

                // Validation de la date de fin
                if (evenementModifie.DateFin != evenementExistant.DateFin)
                {
                    if (evenementModifie.DateFin <= evenementModifie.DateDebut)
                    {
                        _logger.LogWarning("[ModifierEvenement] Date de fin invalide : {DateFin} <= {DateDebut}", 
                            evenementModifie.DateFin, evenementModifie.DateDebut);
                        return BadRequest(new { message = "La date de fin doit être postérieure à la date de début" });
                    }
                    modifications.Add("Date de fin");
                }

                // Validation du statut
                if (evenementModifie.Statut != evenementExistant.Statut)
                {
                    // Vérifier que le nouveau statut est valide
                    var statutsValides = new[] { StatutEvenement.Planifie, StatutEvenement.Annule };
                    if (!statutsValides.Contains(evenementModifie.Statut))
                    {
                        _logger.LogWarning("[ModifierEvenement] Statut invalide : {Statut}", evenementModifie.Statut);
                        return BadRequest(new { 
                            message = "Seuls les statuts 'Planifié' et 'Annulé' sont autorisés",
                            statutPropose = evenementModifie.Statut.ToString(),
                            statutsAutorises = statutsValides.Select(s => s.ToString()).ToArray()
                        });
                    }
                    modifications.Add("Statut");
                }

                // 8. Application des modifications (seulement les champs autorisés)
                evenementExistant.DateDebut = evenementModifie.DateDebut;
                evenementExistant.DateFin = evenementModifie.DateFin;
                evenementExistant.Lieu = evenementModifie.Lieu;
                evenementExistant.Statut = evenementModifie.Statut;

                // 9. Sauvegarde
                await _context.SaveChangesAsync();

                _logger.LogInformation("[ModifierEvenement] Événement {EvenementId} modifié avec succès par {CodePermanent}. Modifications: {Modifications}", 
                    evenementId, codePermanentUtilisateur, string.Join(", ", modifications));

                return Ok(new
                {
                    message = "Événement modifié avec succès",
                    evenement = new
                    {
                        evenementExistant.Id,
                        evenementExistant.Titre,
                        evenementExistant.Description,
                        evenementExistant.DateDebut,
                        evenementExistant.DateFin,
                        evenementExistant.Lieu,
                        evenementExistant.CapaciteMax,
                        evenementExistant.Statut,
                        nombreParticipants = evenementExistant.Participants?.Count ?? 0
                    },
                    modifications = modifications,
                    modifiePar = new
                    {
                        codePermanent = membre.CodePermanent,
                        role = membre.Role.ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ModifierEvenement] Erreur lors de la modification de l'événement {EvenementId} de l'association {AssociationId}", 
                    evenementId, id);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la modification de l'événement" });
            }
        }

        [HttpDelete("SupprimerEvenement/{id}/{evenementId}")]
        public async Task<IActionResult> SupprimerEvenement(int id, int evenementId, [FromQuery] string codePermanentUtilisateur)
        {
            try
            {
                _logger.LogInformation("[SupprimerEvenement] Suppression de l'événement {EvenementId} de l'association {AssociationId} par {CodePermanent}", 
                    evenementId, id, codePermanentUtilisateur);

                // 1. Validation des paramètres
                if (string.IsNullOrWhiteSpace(codePermanentUtilisateur))
                {
                    _logger.LogWarning("[SupprimerEvenement] Code permanent utilisateur non fourni");
                    return BadRequest(new { message = "Le code permanent de l'utilisateur est obligatoire" });
                }

                // 2. Validation de l'association
                var association = await _context.Associations
                    .Include(a => a.Evenements)
                    .Include(a => a.Membres)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (association == null)
                {
                    _logger.LogWarning("[SupprimerEvenement] Association {AssociationId} non trouvée", id);
                    return NotFound(new { message = "Association non trouvée" });
                }

                // 3. Validation de l'événement
                var evenementASupprimer = association.Evenements?
                    .FirstOrDefault(e => e.Id == evenementId);

                if (evenementASupprimer == null)
                {
                    _logger.LogWarning("[SupprimerEvenement] Événement {EvenementId} non trouvé dans l'association {AssociationId}", evenementId, id);
                    return NotFound(new { 
                        message = "Événement non trouvé",
                        evenementId = evenementId,
                        associationId = id
                    });
                }

                // 4. Vérification du timing (événement pas encore commencé)
                if (evenementASupprimer.DateDebut <= DateTime.Now)
                {
                    _logger.LogWarning("[SupprimerEvenement] Événement {EvenementId} a déjà commencé (début: {DateDebut})", 
                        evenementId, evenementASupprimer.DateDebut);
                    return BadRequest(new { 
                        message = "Impossible de supprimer un événement qui a déjà commencé",
                        dateDebut = evenementASupprimer.DateDebut,
                        maintenant = DateTime.Now,
                        suggestion = "Vous pouvez annuler l'événement au lieu de le supprimer"
                    });
                }

                // 5. Vérification des droits de l'utilisateur
                var membre = association.Membres?
                    .FirstOrDefault(m => m.CodePermanent == codePermanentUtilisateur);

                if (membre == null)
                {
                    _logger.LogWarning("[SupprimerEvenement] Utilisateur {CodePermanent} n'est pas membre de l'association {AssociationId}", 
                        codePermanentUtilisateur, id);
                    return NotFound(new { 
                        message = "Vous n'êtes pas membre de cette association",
                        codePermanent = codePermanentUtilisateur,
                        associationId = id
                    });
                }

                // Vérification du statut du membre
                if (membre.Statut != StatutMembre.Actif)
                {
                    _logger.LogWarning("[SupprimerEvenement] Utilisateur {CodePermanent} n'est pas membre actif (statut: {Statut})", 
                        codePermanentUtilisateur, membre.Statut);
                    return Unauthorized(new { 
                        message = "Vous devez être membre actif pour supprimer un événement",
                        statutActuel = membre.Statut.ToString()
                    });
                }

                // 6. Vérification des permissions (seuls les rôles de direction peuvent supprimer)
                var rolesAutorises = new[] { RoleMembre.President, RoleMembre.VicePresident };
                if (!rolesAutorises.Contains(membre.Role))
                {
                    _logger.LogWarning("[SupprimerEvenement] Utilisateur {CodePermanent} n'a pas les droits pour supprimer un événement (rôle: {Role})", 
                        codePermanentUtilisateur, membre.Role);
                    return Unauthorized(new { 
                        message = "Seuls les membres President et VicePresident peuvent supprimer des événements",
                        roleActuel = membre.Role.ToString(),
                        rolesAutorises = rolesAutorises.Select(r => r.ToString()).ToArray()
                    });
                }

                // 7. Vérification que l'utilisateur n'est pas participant
                if (evenementASupprimer.Participants?.Contains(codePermanentUtilisateur) == true)
                {
                    _logger.LogWarning("[SupprimerEvenement] Utilisateur {CodePermanent} est participant de l'événement {EvenementId}", 
                        codePermanentUtilisateur, evenementId);
                    return Unauthorized(new { 
                        message = "Les participants ne peuvent pas supprimer un événement",
                        codePermanent = codePermanentUtilisateur,
                        evenementId = evenementId
                    });
                }

                // 8. Sauvegarde des informations avant suppression pour le log
                var evenementInfo = new
                {
                    evenementASupprimer.Id,
                    evenementASupprimer.Titre,
                    evenementASupprimer.DateDebut,
                    evenementASupprimer.DateFin,
                    evenementASupprimer.Statut,
                    nombreParticipants = evenementASupprimer.Participants?.Count ?? 0
                };

                // 9. Suppression de l'événement
                association.Evenements.Remove(evenementASupprimer);
                await _context.SaveChangesAsync();

                _logger.LogInformation("[SupprimerEvenement] Événement {EvenementId} supprimé avec succès par {CodePermanent}", 
                    evenementId, codePermanentUtilisateur);

                return Ok(new
                {
                    message = "Événement supprimé avec succès",
                    evenementSupprime = evenementInfo,
                    association = new
                    {
                        association.Id,
                        association.Nom,
                        nombreEvenements = association.Evenements.Count
                    },
                    supprimePar = new
                    {
                        codePermanent = membre.CodePermanent,
                        role = membre.Role.ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SupprimerEvenement] Erreur lors de la suppression de l'événement {EvenementId} de l'association {AssociationId}", 
                    evenementId, id);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la suppression de l'événement" });
            }
        }

        [HttpGet("ObtenirDemandesAdhesion/{id}")]
        public async Task<IActionResult> ObtenirDemandesAdhesion(int id, [FromQuery] string codePermanentUtilisateur)
        {
            try
            {
                _logger.LogInformation("[ObtenirDemandesAdhesion] Récupération des demandes d'adhésion pour l'association {Id}", id);

                if (string.IsNullOrWhiteSpace(codePermanentUtilisateur))
                {
                    return BadRequest(new { message = "Le code permanent de l'utilisateur est obligatoire" });
                }

                // Récupérer l'association avec ses membres et demandes
                var association = await _context.Associations
                    .Include(a => a.Membres)
                    .Include(a => a.DemandesAdhesion)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (association == null)
                {
                    return NotFound(new { message = "Association non trouvée" });
                }

                // Vérifier si l'utilisateur est président ou vice-président
                var membre = association.Membres
                    .FirstOrDefault(m => m.CodePermanent == codePermanentUtilisateur);

                if (membre == null || (membre.Role != RoleMembre.President && membre.Role != RoleMembre.VicePresident))
                {
                    return Unauthorized(new { message = "Seuls le président et le vice-président peuvent voir les demandes d'adhésion" });
                }

                var demandes = association.DemandesAdhesion?
                    .Where(d => d.Statut == StatutDemandeAdhesion.EnAttente)
                    .OrderByDescending(d => d.DateDemande)
                    .ToList() ?? new List<DemandeAdhesion>();

                _logger.LogInformation("[ObtenirDemandesAdhesion] {Count} demandes d'adhésion récupérées pour l'association {Id}", demandes.Count, id);

                return Ok(new { 
                    message = "Demandes d'adhésion récupérées avec succès",
                    demandes = demandes.Select(d => new
                    {
                        d.Id,
                        d.CodePermanent,
                        d.Role,
                        d.DateDemande,
                        d.Motivation,
                        d.Statut
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ObtenirDemandesAdhesion] Erreur lors de la récupération des demandes d'adhésion pour l'association {Id}", id);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération des demandes d'adhésion" });
            }
        }

    }
} 

