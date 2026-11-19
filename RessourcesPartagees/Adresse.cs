using System.ComponentModel.DataAnnotations;

namespace RessourcesPartagees
{
    /// <summary>
    /// Adresse d'un compte, avec son propre identifiant.
    /// L'appartement est facultatif et n'a pas de longueur maximale.
    /// Le code permanent lié accepte 450 caractères.
    /// </summary>
    public class Adresse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string NumeroCivique { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Rue { get; set; } = string.Empty;

        /// <summary>Facultatif. Aucune longueur maximale n'est imposée.</summary>
        public string? Appartement { get; set; }

        [Required]
        [MaxLength(50)]
        public string Ville { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Province { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string CodePostal { get; set; } = string.Empty;

        /// <summary>Code du compte lié. Plafond de 450 caractères, plus large que le code du compte.</summary>
        [Required]
        [MaxLength(450)]
        public string CodePermanentUtilisateur { get; set; } = string.Empty;
    }
} 