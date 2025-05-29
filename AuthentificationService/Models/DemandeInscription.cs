using RessourcesPartagees;
using RessourcesPartagees.Enumerations;

namespace AuthentificationService.Models
{
    public class DemandeInscription
    {
        public string CodePermanent { get; set; }
        public string Email { get; set; }
        public string MotDePasse { get; set; }
        public string Prenom { get; set; }
        public string Nom { get; set; }
        public string Programme { get; set; }
        public string Faculte { get; set; }
        public int AnneeDebut { get; set; }
        public int AnneeFin { get; set; }
        public double Telephone { get; set; }
        public RoleUtilisateur Role { get; set; }
        public Adresse Adresse { get; set; }
    }
} 