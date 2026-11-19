using System.ComponentModel.DataAnnotations;
using AssociationService.Models.Enumerations;
using RessourcesPartagees.Enumerations;

namespace AssociationService.Models
{
    public class Membre
    {
        public int Id { get; set; }
        public int AssociationId { get; set; }

        [Required]
        [StringLength(20)]
        public string CodePermanent { get; set; } = string.Empty;

        public RoleMembre Role { get; set; }
        public DateTime DateAdhesion { get; set; }
        public StatutMembre Statut { get; set; }
        public List<string> Responsabilites { get; set; } = new();
        public List<string> Associations { get; set; } = new();
        public RoleUtilisateur RoleUtilisateur { get; set; }
    }
}
