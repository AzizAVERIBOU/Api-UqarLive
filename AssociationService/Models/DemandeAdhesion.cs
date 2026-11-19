using System.ComponentModel.DataAnnotations;
using AssociationService.Models.Enumerations;

namespace AssociationService.Models
{
    public class DemandeAdhesion
    {
        public int Id { get; set; }
        public int AssociationId { get; set; }

        [Required]
        [StringLength(20)]
        public string CodePermanent { get; set; } = string.Empty;

        public RoleMembre Role { get; set; }
        public DateTime DateDemande { get; set; }

        [Required]
        [StringLength(1000)]
        public string Motivation { get; set; } = string.Empty;

        public StatutDemandeAdhesion Statut { get; set; }
    }
}
