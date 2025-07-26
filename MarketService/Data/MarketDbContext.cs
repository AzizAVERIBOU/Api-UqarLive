using Microsoft.EntityFrameworkCore;
using MarketService.Models;

namespace MarketService.Data
{
    public class MarketDbContext : DbContext
    {
        public MarketDbContext(DbContextOptions<MarketDbContext> options) : base(options) { }

        public DbSet<Produit> Produits { get; set; }
        public DbSet<Categorie> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration de la relation Produit-Categorie
            modelBuilder.Entity<Produit>()
                .HasOne(p => p.Categorie)
                .WithMany(c => c.Produits)
                .HasForeignKey(p => p.CategorieId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuration des énumérations
            modelBuilder.Entity<Produit>()
                .Property(p => p.Statut)
                .HasConversion<string>();

            modelBuilder.Entity<Produit>()
                .Property(p => p.ModePaiementSouhaite)
                .HasConversion<string>();

            // Index pour améliorer les performances
            modelBuilder.Entity<Produit>()
                .HasIndex(p => p.CodePermanentVendeur);

            modelBuilder.Entity<Produit>()
                .HasIndex(p => p.Statut);

            modelBuilder.Entity<Produit>()
                .HasIndex(p => p.CategorieId);
        }
    }
} 