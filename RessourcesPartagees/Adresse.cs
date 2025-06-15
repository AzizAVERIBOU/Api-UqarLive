using System.ComponentModel.DataAnnotations;

namespace RessourcesPartagees
{
    public class Adresse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string NumeroCivique { get; set; }

        [Required]
        [MaxLength(100)]
        public string Rue { get; set; }

        public string? Appartement { get; set; }

        [Required]
        [MaxLength(50)]
        public string Ville { get; set; }

        [Required]
        [MaxLength(50)]
        public string Province { get; set; }

        [Required]
        [MaxLength(10)]
        public string CodePostal { get; set; }

        [Required]
        [MaxLength(450)]
        public string CodePermanentUtilisateur { get; set; }
    }
} 