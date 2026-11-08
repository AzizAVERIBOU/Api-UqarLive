using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BibliothequeService.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        public int SalleId { get; set; }

        [Required]
        [MaxLength(20)]
        public string CodePermanentReservateur { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string TitreReservation { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateOnly DateReservation { get; set; }

        [Range(1, 29)]
        public HeureReservation HeureDebut { get; set; }

        [Range(1, 29)]
        public HeureReservation HeureFin { get; set; }

        public StatutReservation Statut { get; set; } = StatutReservation.EnAttente;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        [Range(1, int.MaxValue)]
        public int? NombreParticipants { get; set; }

        [MaxLength(500)]
        public string? Commentaire { get; set; }

        [JsonIgnore]
        public Salle Salle { get; set; } = null!;
    }
}
