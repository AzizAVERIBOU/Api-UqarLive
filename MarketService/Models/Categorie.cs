using System.ComponentModel.DataAnnotations;

namespace MarketService.Models
{
    public class Categorie
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool EstActive { get; set; } = true;

        // Navigation property pour les produits
        public virtual ICollection<Produit> Produits { get; set; } = new List<Produit>();
    }
} 