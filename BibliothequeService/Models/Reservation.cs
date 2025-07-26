using System.ComponentModel.DataAnnotations;

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
        
        public DateTime DateReservation { get; set; }
        
        public HeureReservation HeureDebut { get; set; }
        
        public HeureReservation HeureFin { get; set; }
        
        public StatutReservation Statut { get; set; } = StatutReservation.EnAttente;
        
        public DateTime DateCreation { get; set; } = DateTime.Now;
        
        [MaxLength(100)]
        public string? NombreParticipants { get; set; }
        
        [MaxLength(500)]
        public string? Commentaire { get; set; }
        
        // Navigation property
        public Salle Salle { get; set; } = null!;
    }
} 