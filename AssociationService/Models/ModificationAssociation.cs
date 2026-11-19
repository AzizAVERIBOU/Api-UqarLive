using System.ComponentModel.DataAnnotations;
using AssociationService.Models.Enumerations;

namespace AssociationService.Models
{
    public class ModificationAssociation
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        public CategorieAssociation Categorie { get; set; }
        public Faculte Faculte { get; set; }

        [Required]
        [StringLength(20)]
        public string CodePermanentCreateur { get; set; } = string.Empty;

        public List<string>? Objectifs { get; set; }
        public List<string>? Activites { get; set; }
    }
}
