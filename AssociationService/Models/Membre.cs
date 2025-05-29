using AssociationService.Models.Enumerations;
using RessourcesPartagees.Enumerations;

namespace AssociationService.Models
{
    public class Membre
    {
        public string CodePermanent { get; set; }
        public RoleMembre Role { get; set; }
        public DateTime DateAdhesion { get; set; }
        public StatutMembre Statut { get; set; }
        public List<string> Responsabilites { get; set; }
        public List<string> Associations { get; set; }
        public RoleUtilisateur RoleUtilisateur { get; set; }
    }
} 