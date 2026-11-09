using System.ComponentModel.DataAnnotations;

namespace RessourcesPartagees
{
    public class AdressePostale
    {
        [Required]
        [MaxLength(10)]
        public string NumeroCivique { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Rue { get; set; } = string.Empty;

        [MaxLength(50)]
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
    }
}
