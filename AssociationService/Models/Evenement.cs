using System.ComponentModel.DataAnnotations;
using AssociationService.Models.Enumerations;
using RessourcesPartagees;

namespace AssociationService.Models
{
    public class Evenement
    {
        public int Id { get; set; }
        public int AssociationId { get; set; }

        [Required]
        [StringLength(100)]
        public string Titre { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public AdressePostale Lieu { get; set; } = new();
        public int? CapaciteMax { get; set; }
        public List<string> Participants { get; set; } = new();
        public StatutEvenement Statut { get; set; }
    }
}
