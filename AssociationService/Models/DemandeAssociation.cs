using AssociationService.Models.Enumerations;

namespace AssociationService.Models
{
    public class DemandeAssociation
    {
        public string Nom { get; set; }
        public string Description { get; set; }
        public CategorieAssociation Categorie { get; set; }
        public Faculte Faculte { get; set; }
        public string CodePermanentCreateur { get; set; }
        public DateTime DateCreation { get; set; }
        public StatutAssociation Statut { get; set; }
    }
} 