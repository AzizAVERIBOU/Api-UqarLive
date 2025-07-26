using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliothequeService.Data;
using BibliothequeService.Models;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace BibliothequeService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class BibliothequeController : ControllerBase
    {
        private readonly BibliothequeDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<BibliothequeController> _logger;

        public BibliothequeController(
            BibliothequeDbContext context, 
            IHttpClientFactory httpClientFactory,
            ILogger<BibliothequeController> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // GET: api/bibliotheque/salles
        [HttpGet("salles")]
        public async Task<ActionResult<IEnumerable<Salle>>> GetSalles()
        {
            return await _context.Salles
                .Where(s => s.EstActive)
                .ToListAsync();
        }

        // GET: api/bibliotheque/salles/{id}
        [HttpGet("salles/{id}")]
        public async Task<ActionResult<Salle>> GetSalle(int id)
        {
            var salle = await _context.Salles
                .Include(s => s.Reservations)
                .FirstOrDefaultAsync(s => s.Id == id && s.EstActive);

            if (salle == null)
            {
                return NotFound();
            }

            return salle;
        }

        // POST: api/bibliotheque/salles
        [HttpPost("salles")]
        public async Task<ActionResult<Salle>> CreateSalle([FromBody] Salle salle)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Salles.Add(salle);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSalle), new { id = salle.Id }, salle);
        }

        // PUT: api/bibliotheque/salles/{id}
        [HttpPut("salles/{id}")]
        public async Task<IActionResult> UpdateSalle(int id, [FromBody] Salle salle)
        {
            if (id != salle.Id)
            {
                return BadRequest();
            }

            var existing = await _context.Salles.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.Nom = salle.Nom;
            existing.Localisation = salle.Localisation;
            existing.TypeSalle = salle.TypeSalle;
            existing.Capacite = salle.Capacite;
            existing.Description = salle.Description;
            existing.EstDisponible = salle.EstDisponible;
            existing.EstActive = salle.EstActive;
            existing.Equipements = salle.Equipements;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SalleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/bibliotheque/salles/{id}
        [HttpDelete("salles/{id}")]
        public async Task<IActionResult> DeleteSalle(int id)
        {
            var salle = await _context.Salles.FindAsync(id);
            if (salle == null)
            {
                return NotFound();
            }

            // Vérifier s'il y a des réservations actives
            var reservationsActives = await _context.Reservations
                .AnyAsync(r => r.SalleId == id && 
                              (r.Statut == StatutReservation.Confirmee || 
                               r.Statut == StatutReservation.EnCours));

            if (reservationsActives)
            {
                return BadRequest("Impossible de supprimer une salle avec des réservations actives");
            }

            salle.EstActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/bibliotheque/reservations
        [HttpGet("reservations")]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            return await _context.Reservations
                .Include(r => r.Salle)
                .ToListAsync();
        }

        // GET: api/bibliotheque/reservations/{id}
        [HttpGet("reservations/{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Salle)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return reservation;
        }

        // POST: api/bibliotheque/reservations
        [HttpPost("reservations")]
        public async Task<ActionResult<Reservation>> CreateReservation([FromBody] Reservation reservation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Vérifier que l'utilisateur existe dans le service d'authentification
            try
            {
                var httpClient = _httpClientFactory.CreateClient("AuthService");
                var response = await httpClient.GetAsync($"api/authentification/utilisateurs/{reservation.CodePermanentReservateur}");
                
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Code permanent invalide ou utilisateur non trouvé");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'utilisateur");
                return StatusCode(500, "Erreur lors de la vérification de l'utilisateur");
            }

            // Vérifier que la salle existe et est disponible
            var salle = await _context.Salles.FindAsync(reservation.SalleId);
            if (salle == null || !salle.EstActive)
            {
                return BadRequest("Salle non trouvée ou inactive");
            }

            if (!salle.EstDisponible)
            {
                return BadRequest("Salle non disponible");
            }

            // Vérifier que l'heure de fin est après l'heure de début
            if (reservation.HeureFin <= reservation.HeureDebut)
            {
                return BadRequest("L'heure de fin doit être après l'heure de début");
            }

            // Vérifier que la date de réservation n'est pas dans le passé
            if (reservation.DateReservation.Date < DateTime.Today)
            {
                return BadRequest("Impossible de réserver pour une date passée");
            }

            // Vérifier les conflits de réservation
            var conflit = await _context.Reservations
                .AnyAsync(r => r.SalleId == reservation.SalleId &&
                              r.DateReservation.Date == reservation.DateReservation.Date &&
                              r.Statut != StatutReservation.Annulee &&
                              r.Statut != StatutReservation.Terminee &&
                              ((r.HeureDebut <= reservation.HeureDebut && r.HeureFin > reservation.HeureDebut) ||
                               (r.HeureDebut < reservation.HeureFin && r.HeureFin >= reservation.HeureFin) ||
                               (r.HeureDebut >= reservation.HeureDebut && r.HeureFin <= reservation.HeureFin)));

            if (conflit)
            {
                return BadRequest("Conflit de réservation pour cette période");
            }

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
        }

        // PUT: api/bibliotheque/reservations/{id}
        [HttpPut("reservations/{id}")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return BadRequest();
            }

            var existing = await _context.Reservations.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.TitreReservation = reservation.TitreReservation;
            existing.Description = reservation.Description;
            existing.DateReservation = reservation.DateReservation;
            existing.HeureDebut = reservation.HeureDebut;
            existing.HeureFin = reservation.HeureFin;
            existing.Statut = reservation.Statut;
            existing.NombreParticipants = reservation.NombreParticipants;
            existing.Commentaire = reservation.Commentaire;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/bibliotheque/reservations/{id}
        [HttpDelete("reservations/{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            reservation.Statut = StatutReservation.Annulee;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/bibliotheque/salles/disponibles
        [HttpGet("salles/disponibles")]
        public async Task<ActionResult<IEnumerable<Salle>>> GetSallesDisponibles(
            [FromQuery] DateTime? date = null,
            [FromQuery] HeureReservation? heureDebut = null,
            [FromQuery] HeureReservation? heureFin = null,
            [FromQuery] TypeSalle? typeSalle = null,
            [FromQuery] int? capaciteMin = null)
        {
            var query = _context.Salles.Where(s => s.EstActive && s.EstDisponible);

            if (typeSalle.HasValue)
            {
                query = query.Where(s => s.TypeSalle == typeSalle.Value);
            }

            if (capaciteMin.HasValue)
            {
                query = query.Where(s => s.Capacite >= capaciteMin.Value);
            }

            var salles = await query.ToListAsync();

            // Filtrer par disponibilité si des paramètres sont spécifiés
            if (date.HasValue && heureDebut.HasValue && heureFin.HasValue)
            {
                var sallesDisponibles = new List<Salle>();
                foreach (var salle in salles)
                {
                    var conflit = await _context.Reservations
                        .AnyAsync(r => r.SalleId == salle.Id &&
                                      r.DateReservation.Date == date.Value.Date &&
                                      r.Statut != StatutReservation.Annulee &&
                                      r.Statut != StatutReservation.Terminee &&
                                      ((r.HeureDebut <= heureDebut.Value && r.HeureFin > heureDebut.Value) ||
                                       (r.HeureDebut < heureFin.Value && r.HeureFin >= heureFin.Value) ||
                                       (r.HeureDebut >= heureDebut.Value && r.HeureFin <= heureFin.Value)));

                    if (!conflit)
                    {
                        sallesDisponibles.Add(salle);
                    }
                }
                return sallesDisponibles;
            }

            return salles;
        }

        // GET: api/bibliotheque/heures
        [HttpGet("heures")]
        public ActionResult<IEnumerable<object>> GetHeuresDisponibles()
        {
            var heures = Enum.GetValues(typeof(HeureReservation))
                .Cast<HeureReservation>()
                .Select(h => new { 
                    Id = (int)h, 
                    Nom = h.ToString(),
                    Heure = GetHeureString(h)
                })
                .ToList();

            return heures;
        }

        private string GetHeureString(HeureReservation heure)
        {
            return heure switch
            {
                HeureReservation.HuitHeures => "08:00",
                HeureReservation.HuitHeuresTrente => "08:30",
                HeureReservation.NeufHeures => "09:00",
                HeureReservation.NeufHeuresTrente => "09:30",
                HeureReservation.DixHeures => "10:00",
                HeureReservation.DixHeuresTrente => "10:30",
                HeureReservation.OnzeHeures => "11:00",
                HeureReservation.OnzeHeuresTrente => "11:30",
                HeureReservation.DouzeHeures => "12:00",
                HeureReservation.DouzeHeuresTrente => "12:30",
                HeureReservation.TreizeHeures => "13:00",
                HeureReservation.TreizeHeuresTrente => "13:30",
                HeureReservation.QuatorzeHeures => "14:00",
                HeureReservation.QuatorzeHeuresTrente => "14:30",
                HeureReservation.QuinzeHeures => "15:00",
                HeureReservation.QuinzeHeuresTrente => "15:30",
                HeureReservation.SeizeHeures => "16:00",
                HeureReservation.SeizeHeuresTrente => "16:30",
                HeureReservation.DixSeptHeures => "17:00",
                HeureReservation.DixSeptHeuresTrente => "17:30",
                HeureReservation.DixHuitHeures => "18:00",
                HeureReservation.DixHuitHeuresTrente => "18:30",
                HeureReservation.DixNeufHeures => "19:00",
                HeureReservation.DixNeufHeuresTrente => "19:30",
                HeureReservation.VingtHeures => "20:00",
                HeureReservation.VingtHeuresTrente => "20:30",
                HeureReservation.VingtEtUneHeures => "21:00",
                HeureReservation.VingtEtUneHeuresTrente => "21:30",
                HeureReservation.VingtDeuxHeures => "22:00",
                _ => "00:00"
            };
        }

        private bool SalleExists(int id)
        {
            return _context.Salles.Any(e => e.Id == id);
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }
    }
} 