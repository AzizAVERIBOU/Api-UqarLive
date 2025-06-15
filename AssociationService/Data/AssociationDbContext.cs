using Microsoft.EntityFrameworkCore;
using AssociationService.Models;

namespace AssociationService.Data
{
    public class AssociationDbContext : DbContext
    {
        public AssociationDbContext(DbContextOptions<AssociationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Association> Associations { get; set; }
        public DbSet<Membre> Membres { get; set; }
        public DbSet<Evenement> Evenements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration de l'entité Association
            modelBuilder.Entity<Association>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.CodePermanentCreateur).IsRequired();
                entity.Property(e => e.DateCreation).IsRequired();
                
                // Relations
                entity.HasMany(e => e.Membres)
                      .WithOne()
                      .HasForeignKey("AssociationId");

                entity.HasMany(e => e.Evenements)
                      .WithOne()
                      .HasForeignKey("AssociationId");
            });

            // Configuration de l'entité Membre
            modelBuilder.Entity<Membre>(entity =>
            {
                entity.HasKey(e => e.CodePermanent);
                entity.Property(e => e.DateAdhesion).IsRequired();
                entity.Property(e => e.Role).IsRequired();
                entity.Property(e => e.Statut).IsRequired();
            });

            // Configuration de l'entité Evenement
            modelBuilder.Entity<Evenement>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Titre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.DateDebut).IsRequired();
                entity.Property(e => e.DateFin).IsRequired();
                entity.Property(e => e.Statut).IsRequired();
            });
        }
    }
} 