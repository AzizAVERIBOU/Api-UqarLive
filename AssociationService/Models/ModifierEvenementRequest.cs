using RessourcesPartagees;
using AssociationService.Models.Enumerations;

namespace AssociationService.Models
{
    public class ModifierEvenementRequest
    {
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public Adresse Lieu { get; set; }
        public StatutEvenement Statut { get; set; }
    }
} 