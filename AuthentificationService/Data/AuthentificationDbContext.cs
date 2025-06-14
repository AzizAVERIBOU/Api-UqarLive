using Microsoft.EntityFrameworkCore;
using AuthentificationService.Models;
using RessourcesPartagees.Enumerations;
using RessourcesPartagees;

namespace AuthentificationService.Data
{
    public class AuthentificationDbContext : DbContext
    {
        public AuthentificationDbContext(DbContextOptions<AuthentificationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Adresse> Adresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration de l'entité Utilisateur
            modelBuilder.Entity<Utilisateur>(entity =>
            {
                // Configuration de la clé primaire
                entity.HasKey(e => e.CodePermanent);

                // Configuration des propriétés requises
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasAnnotation("RegularExpression", @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

                entity.Property(e => e.MotDePasseHash)
                    .IsRequired();

                entity.Property(e => e.MotDePasseSalt)
                    .IsRequired();

                entity.Property(e => e.Nom)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Prenom)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasConversion<string>();

                // Configuration des index
                entity.HasIndex(e => e.Email)
                    .IsUnique();

                // Configuration des valeurs par défaut
                entity.Property(e => e.DateCreation)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.EstActif)
                    .HasDefaultValue(true);
            });

            // Configuration de l'entité Adresse
            modelBuilder.Entity<Adresse>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NumeroCivique).HasMaxLength(10).IsRequired();
                entity.Property(e => e.Rue).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Ville).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Province).HasMaxLength(50).IsRequired();
                entity.Property(e => e.CodePostal).HasMaxLength(10).IsRequired();
                entity.Property(e => e.CodePermanentUtilisateur).HasMaxLength(450).IsRequired();

                // Configuration de la relation avec Utilisateur
                entity.HasOne<Utilisateur>()
                    .WithMany(u => u.Adresses)
                    .HasForeignKey(a => a.CodePermanentUtilisateur)
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired();
            });
        }
    }
}
