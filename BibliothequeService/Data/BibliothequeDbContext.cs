using Microsoft.EntityFrameworkCore;
using BibliothequeService.Models;

namespace BibliothequeService.Data
{
    public class BibliothequeDbContext : DbContext
    {
        public BibliothequeDbContext(DbContextOptions<BibliothequeDbContext> options)
            : base(options)
        {
        }

        public DbSet<Salle> Salles { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration des relations
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Salle)
                .WithMany(s => s.Reservations)
                .HasForeignKey(r => r.SalleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuration des énumérations
            modelBuilder.Entity<Salle>()
                .Property(s => s.TypeSalle)
                .HasConversion<string>();

            modelBuilder.Entity<Reservation>()
                .Property(r => r.Statut)
                .HasConversion<string>();

            modelBuilder.Entity<Reservation>()
                .Property(r => r.HeureDebut)
                .HasConversion<string>();

            modelBuilder.Entity<Reservation>()
                .Property(r => r.HeureFin)
                .HasConversion<string>();

            // Index pour les performances
            modelBuilder.Entity<Salle>()
                .HasIndex(s => s.Nom)
                .IsUnique();

            modelBuilder.Entity<Reservation>()
                .HasIndex(r => r.CodePermanentReservateur);

            modelBuilder.Entity<Reservation>()
                .HasIndex(r => r.DateCreation);

            modelBuilder.Entity<Reservation>()
                .HasIndex(r => r.HeureDebut);

            modelBuilder.Entity<Reservation>()
                .HasIndex(r => r.HeureFin);

            // Configuration des contraintes
            modelBuilder.Entity<Salle>()
                .Property(s => s.Capacite)
                .HasDefaultValue(1);
        }
    }
} 