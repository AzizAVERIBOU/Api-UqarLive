using Microsoft.EntityFrameworkCore;
using BibliothequeService.Models;

namespace BibliothequeService.Data
{
    public class BibliothequeDbContext : DbContext
    {
        public const string HistoriqueMigrations = "__BibliothequeMigrationsHistory";

        public BibliothequeDbContext(DbContextOptions<BibliothequeDbContext> options)
            : base(options)
        {
        }

        public DbSet<Salle> Salles { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        public static void UtiliserPostgres(DbContextOptionsBuilder options, string? connectionString)
        {
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable(HistoriqueMigrations));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Salle)
                .WithMany(s => s.Reservations)
                .HasForeignKey(r => r.SalleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Salle>(salle =>
            {
                salle.Property(s => s.TypeSalle)
                    .HasConversion<string>();

                salle.ToTable("Salles", table =>
                    table.HasCheckConstraint("CK_Salles_Capacite", "\"Capacite\" >= 1"));

                salle.HasIndex(s => s.Nom)
                    .IsUnique();
            });

            modelBuilder.Entity<Reservation>(reservation =>
            {
                reservation.Property(r => r.Statut)
                    .HasConversion<string>();

                reservation.Property(r => r.HeureDebut)
                    .HasConversion<int>();

                reservation.Property(r => r.HeureFin)
                    .HasConversion<int>();

                reservation.Property(r => r.DateReservation)
                    .HasColumnType("date");

                reservation.ToTable("Reservations", table =>
                {
                    table.HasCheckConstraint(
                        "CK_Reservations_Heures",
                        "\"HeureDebut\" BETWEEN 1 AND 29 AND \"HeureFin\" BETWEEN 1 AND 29 AND \"HeureFin\" > \"HeureDebut\"");
                    table.HasCheckConstraint(
                        "CK_Reservations_Participants",
                        "\"NombreParticipants\" IS NULL OR \"NombreParticipants\" >= 1");
                });

                reservation.HasIndex(r => r.CodePermanentReservateur);
                reservation.HasIndex(r => r.DateCreation);
                reservation.HasIndex(r => r.HeureDebut);
                reservation.HasIndex(r => r.HeureFin);
            });
        }
    }
}
