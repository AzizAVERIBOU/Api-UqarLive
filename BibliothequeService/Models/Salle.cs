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

        [Range(1, 8)]
        public TypeSalle TypeSalle { get; set; }

        [Range(1, int.MaxValue)]
        public int Capacite { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool EstDisponible { get; set; } = true;

        public bool EstActive { get; set; } = true;

        [MaxLength(200)]
        public string? Equipements { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
