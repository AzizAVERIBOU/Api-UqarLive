using AssociationService.Models.Enumerations;

namespace AssociationService.Models
{
    public class ModificationAssociation
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Description { get; set; }
        public CategorieAssociation Categorie { get; set; }
        public Faculte Faculte { get; set; }
        public string CodePermanentCreateur { get; set; }
        public List<string>? Objectifs { get; set; }
        public List<string>? Activites { get; set; }
    }
} 