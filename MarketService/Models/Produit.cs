using System.ComponentModel.DataAnnotations;

namespace MarketService.Models
{
    public class Produit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Titre { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Range(0, 10000)]
        public decimal Prix { get; set; }

        [Required]
        [StringLength(20)]
        public string CodePermanentVendeur { get; set; } = string.Empty;

        [Required]
        public StatutProduit Statut { get; set; } = StatutProduit.Disponible;

        [Required]
        public ModePaiement ModePaiementSouhaite { get; set; } = ModePaiement.Especes;

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        public DateTime? DateVente { get; set; }

        [StringLength(500)]
        public string? ContactVendeur { get; set; }

        [StringLength(100)]
        public string? Localisation { get; set; }

        // Clé étrangère pour la catégorie
        public int CategorieId { get; set; }
        public virtual Categorie Categorie { get; set; } = null!;
    }
} 