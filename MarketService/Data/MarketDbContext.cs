using Microsoft.EntityFrameworkCore;
using MarketService.Models;

namespace MarketService.Data
{
    public class MarketDbContext : DbContext
    {
        public const string HistoriqueMigrations = "__MarcheMigrationsHistory";

        public MarketDbContext(DbContextOptions<MarketDbContext> options) : base(options) { }

        public DbSet<Produit> Produits { get; set; }
        public DbSet<Categorie> Categories { get; set; }

        public static void UtiliserPostgres(DbContextOptionsBuilder options, string? connectionString)
        {
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable(HistoriqueMigrations));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Produit>()
                .HasOne(p => p.Categorie)
                .WithMany(c => c.Produits)
                .HasForeignKey(p => p.CategorieId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Categorie>(categorie =>
            {
                categorie.Property(c => c.Nom)
                    .IsRequired()
                    .HasMaxLength(100);

                categorie.Property(c => c.Description)
                    .HasMaxLength(500);
            });

            modelBuilder.Entity<Produit>(produit =>
            {
                produit.Property(p => p.Titre)
                    .IsRequired()
                    .HasMaxLength(200);

                produit.Property(p => p.Description)
                    .HasMaxLength(1000);

                produit.Property(p => p.Prix)
                    .HasPrecision(7, 2);

                produit.Property(p => p.CodePermanentVendeur)
                    .IsRequired()
                    .HasMaxLength(20);

                produit.Property(p => p.Statut)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                produit.Property(p => p.ModePaiementSouhaite)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                produit.Property(p => p.ContactVendeur)
                    .HasMaxLength(500);

                produit.Property(p => p.Localisation)
                    .HasMaxLength(100);

                produit.ToTable("Produits", table =>
                    table.HasCheckConstraint(
                        "CK_Produits_Prix",
                        "\"Prix\" >= 0 AND \"Prix\" <= 10000"));

                produit.HasIndex(p => p.CodePermanentVendeur);
                produit.HasIndex(p => p.Statut);
                produit.HasIndex(p => p.CategorieId);
            });
        }
    }
}
