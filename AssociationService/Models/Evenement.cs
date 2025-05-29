using RessourcesPartagees;
using AssociationService.Models.Enumerations;

namespace AssociationService.Models
{
    public class Evenement
    {
        public int Id { get; set; }
        public string Titre { get; set; }
        public string Description { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public Adresse Lieu { get; set; }
        public int? CapaciteMax { get; set; }
        public List<string>? Participants { get; set; }
        public StatutEvenement Statut { get; set; }
    }
} 