using AssociationService.Models.Enumerations;

namespace AssociationService.Models
{
    public class DemandeAdhesion
    {
        public string CodePermanent { get; set; }
        public RoleMembre Role { get; set; }
        public DateTime DateDemande { get; set; }
        public string Motivation { get; set; }
    }
} 