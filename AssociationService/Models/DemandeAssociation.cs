using System.ComponentModel.DataAnnotations;
using AssociationService.Models.Enumerations;

namespace AssociationService.Models
{
    public class DemandeAssociation
    {
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

        public DateTime DateCreation { get; set; }
        public StatutAssociation Statut { get; set; }
    }
}
