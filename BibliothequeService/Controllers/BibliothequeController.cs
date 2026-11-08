using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliothequeService.Data;
using BibliothequeService.Models;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Npgsql;

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

            if (await NomDeSallePrisAsync(salle.Nom))
            {
                return BadRequest("Une salle porte déjà ce nom");
            }

            salle.Id = 0;
            salle.DateCreation = DateTime.UtcNow;
            _context.Salles.Add(salle);
            var doublon = await EnregistrerSalleAsync();
            if (doublon != null)
            {
                return doublon;
            }

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

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _context.Salles.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            if (await NomDeSallePrisAsync(salle.Nom, id))
            {
                return BadRequest("Une salle porte déjà ce nom");
            }

            if (!salle.EstActive && await SalleOccupeeAsync(id))
            {
                return BadRequest("Impossible de désactiver une salle qui a une réservation en attente, confirmée ou en cours");
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
            catch (DbUpdateException ex) when (EstViolationUnique(ex))
            {
                return BadRequest("Une salle porte déjà ce nom");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SalleExists(id))
                {
                    return NotFound();
                }

                throw;
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

            if (await SalleOccupeeAsync(id))
            {
                return BadRequest("Impossible de désactiver une salle qui a une réservation en attente, confirmée ou en cours");
            }

            salle.EstActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/bibliotheque/reservations
        [HttpGet("reservations")]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            return await _context.Reservations.ToListAsync();
        }

        // GET: api/bibliotheque/reservations/{id}
        [HttpGet("reservations/{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(int id)
        {
            var reservation = await _context.Reservations
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

            var compte = await CompteActifAsync(reservation.CodePermanentReservateur);
            if (compte != null)
            {
                return compte;
            }

            var salle = await _context.Salles.FindAsync(reservation.SalleId);
            var salleRefusee = SalleReservable(salle);
            if (salleRefusee != null)
            {
                return salleRefusee;
            }

            var creneau = ValiderCreneau(reservation.HeureDebut, reservation.HeureFin, reservation.DateReservation);
            if (creneau != null)
            {
                return creneau;
            }

            var participants = ValiderParticipants(reservation.NombreParticipants, salle!.Capacite);
            if (participants != null)
            {
                return participants;
            }

            if (await CreneauOccupeAsync(reservation.SalleId, reservation.DateReservation, reservation.HeureDebut, reservation.HeureFin))
            {
                return BadRequest("Conflit de réservation pour cette période");
            }

            reservation.Id = 0;
            reservation.Statut = StatutReservation.EnAttente;
            reservation.DateCreation = DateTime.UtcNow;
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

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _context.Reservations.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            if (reservation.HeureFin <= reservation.HeureDebut)
            {
                return BadRequest("L'heure de fin doit être après l'heure de début");
            }

            if (OccupeLeCreneau(reservation.Statut))
            {
                var salle = await _context.Salles.FindAsync(existing.SalleId);
                var salleRefusee = SalleReservable(salle);
                if (salleRefusee != null)
                {
                    return salleRefusee;
                }

                if (reservation.DateReservation < AujourdhuiQuebec())
                {
                    return BadRequest("Impossible de réserver pour une date passée");
                }

                var participants = ValiderParticipants(reservation.NombreParticipants, salle!.Capacite);
                if (participants != null)
                {
                    return participants;
                }

                if (await CreneauOccupeAsync(existing.SalleId, reservation.DateReservation, reservation.HeureDebut, reservation.HeureFin, existing.Id))
                {
                    return BadRequest("Conflit de réservation pour cette période");
                }
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

                throw;
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
                if (heureFin.Value <= heureDebut.Value)
                {
                    return BadRequest("L'heure de fin doit être après l'heure de début");
                }

                var jour = DateOnly.FromDateTime(date.Value);
                var sallesDisponibles = new List<Salle>();
                foreach (var salleDisponible in salles)
                {
                    if (!await CreneauOccupeAsync(salleDisponible.Id, jour, heureDebut.Value, heureFin.Value))
                    {
                        sallesDisponibles.Add(salleDisponible);
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

        private async Task<ActionResult?> CompteActifAsync(string codePermanent)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("AuthService");
                var response = await httpClient.GetAsync($"api/Authentification/VerifierUtilisateur/{codePermanent}");
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Code permanent invalide ou utilisateur non trouvé");
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'utilisateur");
                return StatusCode(500, "Erreur lors de la vérification de l'utilisateur");
            }
        }

        private ActionResult? SalleReservable(Salle? salle)
        {
            if (salle == null || !salle.EstActive)
            {
                return BadRequest("Salle non trouvée ou inactive");
            }

            if (!salle.EstDisponible)
            {
                return BadRequest("Salle non disponible");
            }

            return null;
        }

        private ActionResult? ValiderCreneau(HeureReservation debut, HeureReservation fin, DateOnly date)
        {
            if (fin <= debut)
            {
                return BadRequest("L'heure de fin doit être après l'heure de début");
            }

            if (date < AujourdhuiQuebec())
            {
                return BadRequest("Impossible de réserver pour une date passée");
            }

            return null;
        }

        private ActionResult? ValiderParticipants(int? nombre, int capacite)
        {
            if (nombre is int valeur && valeur > capacite)
            {
                return BadRequest("Le nombre de participants dépasse la capacité de la salle");
            }

            return null;
        }

        private Task<bool> CreneauOccupeAsync(
            int salleId,
            DateOnly date,
            HeureReservation debut,
            HeureReservation fin,
            int? reservationIgnoree = null)
        {
            return _context.Reservations.AnyAsync(r =>
                r.SalleId == salleId
                && r.DateReservation == date
                && (reservationIgnoree == null || r.Id != reservationIgnoree)
                && r.Statut != StatutReservation.Annulee
                && r.Statut != StatutReservation.Terminee
                && r.HeureDebut < fin
                && debut < r.HeureFin);
        }

        private Task<bool> SalleOccupeeAsync(int salleId)
        {
            return _context.Reservations.AnyAsync(r =>
                r.SalleId == salleId
                && r.Statut != StatutReservation.Annulee
                && r.Statut != StatutReservation.Terminee);
        }

        private Task<bool> NomDeSallePrisAsync(string nom, int? salleIgnoree = null)
        {
            return _context.Salles.AnyAsync(s =>
                s.Nom == nom && (salleIgnoree == null || s.Id != salleIgnoree));
        }

        private async Task<ActionResult?> EnregistrerSalleAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                return null;
            }
            catch (DbUpdateException ex) when (EstViolationUnique(ex))
            {
                return BadRequest("Une salle porte déjà ce nom");
            }
        }

        private static bool OccupeLeCreneau(StatutReservation statut)
        {
            return statut != StatutReservation.Annulee && statut != StatutReservation.Terminee;
        }

        private static bool EstViolationUnique(DbUpdateException ex)
        {
            return ex.InnerException is PostgresException postgres
                && postgres.SqlState == PostgresErrorCodes.UniqueViolation;
        }

        private static DateOnly AujourdhuiQuebec()
        {
            TimeZoneInfo zone;
            try
            {
                zone = TimeZoneInfo.FindSystemTimeZoneById("America/Toronto");
            }
            catch (TimeZoneNotFoundException)
            {
                zone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            }

            return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone));
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