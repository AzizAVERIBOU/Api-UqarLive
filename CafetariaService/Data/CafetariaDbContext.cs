using Microsoft.EntityFrameworkCore;
using CafetariaService.Models;

namespace CafetariaService.Data
{
    public class CafetariaDbContext : DbContext
    {
        public const string HistoriqueMigrations = "__CafetariaMigrationsHistory";

        public CafetariaDbContext(DbContextOptions<CafetariaDbContext> options) : base(options) { }

        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<Horaire> Horaires { get; set; }

        public static void UtiliserPostgres(DbContextOptionsBuilder options, string? connectionString)
        {
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable(HistoriqueMigrations));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MenuItem>(plat =>
            {
                plat.Property(p => p.Nom)
                    .IsRequired()
                    .HasMaxLength(100);

                plat.Property(p => p.Description)
                    .HasMaxLength(500);

                plat.Property(p => p.Prix)
                    .HasPrecision(6, 2);

                plat.ToTable("MenuItems", table =>
                    table.HasCheckConstraint("CK_MenuItems_Prix", "\"Prix\" >= 0 AND \"Prix\" <= 1000"));
            });

            modelBuilder.Entity<Menu>(menu =>
            {
                menu.Property(m => m.Description)
                    .HasMaxLength(500);

                menu.Property(m => m.Date)
                    .HasColumnType("date");

                menu.HasIndex(m => m.Date)
                    .IsUnique();

                menu.HasMany(m => m.PlatsDuJour)
                    .WithMany()
                    .UsingEntity(jointure => jointure.ToTable("MenusPlats"));
            });

            modelBuilder.Entity<Horaire>(horaire =>
            {
                horaire.Property(h => h.JourDeLaSemaine)
                    .HasConversion<int>();

                horaire.HasIndex(h => h.JourDeLaSemaine)
                    .IsUnique();

                horaire.ToTable("Horaires", table =>
                    table.HasCheckConstraint(
                        "CK_Horaires_Creneau",
                        "\"JourDeLaSemaine\" BETWEEN 1 AND 5 AND \"HeureFermeture\" > \"HeureOuverture\""));
            });
        }
    }
}
