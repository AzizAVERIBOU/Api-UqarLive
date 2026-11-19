using AssociationService.Models;
using Microsoft.EntityFrameworkCore;

namespace AssociationService.Data
{
    public class AssociationDbContext : DbContext
    {
        public const string HistoriqueMigrations = "__AssociationMigrationsHistory";

        public AssociationDbContext(DbContextOptions<AssociationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Association> Associations { get; set; }
        public DbSet<Membre> Membres { get; set; }
        public DbSet<Evenement> Evenements { get; set; }
        public DbSet<DemandeAdhesion> DemandesAdhesion { get; set; }

        public static void UtiliserPostgres(DbContextOptionsBuilder options, string? connectionString)
        {
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable(HistoriqueMigrations));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasPostgresExtension("citext");

            modelBuilder.Entity<Association>(association =>
            {
                association.Property(a => a.Nom)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("citext");

                association.Property(a => a.Description)
                    .IsRequired()
                    .HasMaxLength(2000);

                association.Property(a => a.CodePermanentCreateur)
                    .IsRequired()
                    .HasMaxLength(20);

                association.Property(a => a.Categorie).HasConversion<int>();
                association.Property(a => a.Faculte).HasConversion<int>();
                association.Property(a => a.Statut).HasConversion<int>();
                association.Property(a => a.DateCreation).IsRequired();

                association.PrimitiveCollection(a => a.Objectifs)
                    .IsRequired()
                    .HasColumnType("text[]");
                association.PrimitiveCollection(a => a.Activites)
                    .IsRequired()
                    .HasColumnType("text[]");

                association.HasIndex(a => a.Nom).IsUnique();

                association.HasMany(a => a.Membres)
                    .WithOne()
                    .HasForeignKey(m => m.AssociationId)
                    .OnDelete(DeleteBehavior.Restrict);

                association.HasMany(a => a.Evenements)
                    .WithOne()
                    .HasForeignKey(e => e.AssociationId)
                    .OnDelete(DeleteBehavior.Restrict);

                association.HasMany(a => a.DemandesAdhesion)
                    .WithOne()
                    .HasForeignKey(d => d.AssociationId)
                    .OnDelete(DeleteBehavior.Restrict);

                association.ToTable("Associations", table =>
                {
                    table.HasCheckConstraint("CK_Associations_Categorie", "\"Categorie\" BETWEEN 0 AND 9");
                    table.HasCheckConstraint("CK_Associations_Faculte", "\"Faculte\" BETWEEN 0 AND 7");
                    table.HasCheckConstraint("CK_Associations_Statut", "\"Statut\" BETWEEN 0 AND 3");
                });
            });

            modelBuilder.Entity<Membre>(membre =>
            {
                membre.Property(m => m.Id).ValueGeneratedOnAdd();
                membre.Property(m => m.CodePermanent).IsRequired().HasMaxLength(20);
                membre.Property(m => m.DateAdhesion).IsRequired();
                membre.Property(m => m.Role).HasConversion<int>();
                membre.Property(m => m.Statut).HasConversion<int>();
                membre.Property(m => m.RoleUtilisateur).HasConversion<int>();

                membre.PrimitiveCollection(m => m.Responsabilites)
                    .IsRequired()
                    .HasColumnType("text[]");
                membre.PrimitiveCollection(m => m.Associations)
                    .IsRequired()
                    .HasColumnType("text[]");

                membre.HasIndex(m => new { m.AssociationId, m.CodePermanent }).IsUnique();

                membre.ToTable("Membres", table =>
                {
                    table.HasCheckConstraint("CK_Membres_Role", "\"Role\" BETWEEN 0 AND 4");
                    table.HasCheckConstraint("CK_Membres_Statut", "\"Statut\" BETWEEN 0 AND 3");
                    table.HasCheckConstraint("CK_Membres_RoleUtilisateur", "\"RoleUtilisateur\" BETWEEN 0 AND 2");
                });
            });

            modelBuilder.Entity<Evenement>(evenement =>
            {
                evenement.Property(e => e.Titre).IsRequired().HasMaxLength(100);
                evenement.Property(e => e.Description).IsRequired().HasMaxLength(2000);
                evenement.Property(e => e.DateDebut).IsRequired();
                evenement.Property(e => e.DateFin).IsRequired();
                evenement.Property(e => e.Statut).HasConversion<int>();

                evenement.PrimitiveCollection(e => e.Participants)
                    .IsRequired()
                    .HasColumnType("text[]");

                evenement.OwnsOne(e => e.Lieu, lieu =>
                {
                    lieu.Property(l => l.NumeroCivique).IsRequired().HasMaxLength(10).HasColumnName("Lieu_NumeroCivique");
                    lieu.Property(l => l.Rue).IsRequired().HasMaxLength(100).HasColumnName("Lieu_Rue");
                    lieu.Property(l => l.Appartement).HasMaxLength(50).HasColumnName("Lieu_Appartement");
                    lieu.Property(l => l.Ville).IsRequired().HasMaxLength(50).HasColumnName("Lieu_Ville");
                    lieu.Property(l => l.Province).IsRequired().HasMaxLength(50).HasColumnName("Lieu_Province");
                    lieu.Property(l => l.CodePostal).IsRequired().HasMaxLength(10).HasColumnName("Lieu_CodePostal");
                });
                evenement.Navigation(e => e.Lieu).IsRequired();

                evenement.ToTable("Evenements", table =>
                {
                    table.HasCheckConstraint("CK_Evenements_Dates", "\"DateFin\" > \"DateDebut\"");
                    table.HasCheckConstraint("CK_Evenements_Capacite", "\"CapaciteMax\" IS NULL OR \"CapaciteMax\" > 0");
                    table.HasCheckConstraint("CK_Evenements_Statut", "\"Statut\" BETWEEN 0 AND 3");
                });
            });

            modelBuilder.Entity<DemandeAdhesion>(demande =>
            {
                demande.Property(d => d.CodePermanent).IsRequired().HasMaxLength(20);
                demande.Property(d => d.Motivation).IsRequired().HasMaxLength(1000);
                demande.Property(d => d.DateDemande).IsRequired();
                demande.Property(d => d.Role).HasConversion<int>();
                demande.Property(d => d.Statut).HasConversion<int>();

                demande.HasIndex(d => new { d.AssociationId, d.CodePermanent })
                    .IsUnique()
                    .HasFilter("\"Statut\" = 0");

                demande.ToTable("DemandesAdhesion", table =>
                {
                    table.HasCheckConstraint("CK_DemandesAdhesion_Role", "\"Role\" BETWEEN 0 AND 4");
                    table.HasCheckConstraint("CK_DemandesAdhesion_Statut", "\"Statut\" BETWEEN 0 AND 2");
                });
            });
        }
    }
}
