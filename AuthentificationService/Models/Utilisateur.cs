using System.ComponentModel.DataAnnotations;
using RessourcesPartagees;
using RessourcesPartagees.Enumerations;
using System.Collections.Generic;

namespace AuthentificationService.Models
{
    public class Utilisateur
    {
        [Key]
        public string CodePermanent { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Nom { get; set; }

        [Required]
        public string Prenom { get; set; }

        [Required]
        public byte[] MotDePasseHash { get; set; }

        [Required]
        public byte[] MotDePasseSalt { get; set; }

        /// <summary>
        /// Rôle du compte. En base, le nom est stocké : Etudiant, Personnel ou Admin.
        /// L'inscription publique force Etudiant.
        /// </summary>
        [Required]
        public RoleUtilisateur Role { get; set; }

        public string Programme { get; set; }
        public string Faculte { get; set; }
        public int AnneeDebut { get; set; }
        public int AnneeFin { get; set; }
        public string Telephone { get; set; }
        public DateTime DateInscription { get; set; }
        public DateTime DerniereConnexion { get; set; }
        public DateTime DateCreation { get; set; }
        public bool EstActif { get; set; }

        public virtual ICollection<Adresse> Adresses { get; set; }
    }
} 