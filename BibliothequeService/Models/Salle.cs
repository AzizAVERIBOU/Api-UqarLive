using System.ComponentModel.DataAnnotations;

namespace BibliothequeService.Models
{
    public class Salle
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Nom { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Localisation { get; set; } = string.Empty;
        
        public TypeSalle TypeSalle { get; set; }
        
        public int Capacite { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public bool EstDisponible { get; set; } = true;
        
        public bool EstActive { get; set; } = true;
        
        [MaxLength(200)]
        public string? Equipements { get; set; }
        
        public DateTime DateCreation { get; set; } = DateTime.Now;
        
        // Navigation properties
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
} 