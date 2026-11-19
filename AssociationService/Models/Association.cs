using System.ComponentModel.DataAnnotations;
using AssociationService.Models.Enumerations;

namespace AssociationService.Models
{
    public class Association
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

        public List<string> Objectifs { get; set; } = new();
        public List<string> Activites { get; set; } = new();
        public DateTime DateCreation { get; set; }
        public StatutAssociation Statut { get; set; }
        public List<Membre> Membres { get; set; } = new();
        public List<Evenement> Evenements { get; set; } = new();
        public List<DemandeAdhesion> DemandesAdhesion { get; set; } = new();
    }
}
