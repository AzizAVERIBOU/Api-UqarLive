using RessourcesPartagees;
using AssociationService.Models.Enumerations;

namespace AssociationService.Models
{
    public class ModifierEvenementRequest
    {
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public AdressePostale Lieu { get; set; } = new();
        public StatutEvenement Statut { get; set; }
    }
} 