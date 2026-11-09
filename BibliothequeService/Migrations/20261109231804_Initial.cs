using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BibliothequeService.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Salles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Localisation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TypeSalle = table.Column<string>(type: "text", nullable: false),
                    Capacite = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EstDisponible = table.Column<bool>(type: "boolean", nullable: false),
                    EstActive = table.Column<bool>(type: "boolean", nullable: false),
                    Equipements = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salles", x => x.Id);
                    table.CheckConstraint("CK_Salles_Capacite", "\"Capacite\" >= 1");
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SalleId = table.Column<int>(type: "integer", nullable: false),
                    CodePermanentReservateur = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TitreReservation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DateReservation = table.Column<DateOnly>(type: "date", nullable: false),
                    HeureDebut = table.Column<int>(type: "integer", nullable: false),
                    HeureFin = table.Column<int>(type: "integer", nullable: false),
                    Statut = table.Column<string>(type: "text", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NombreParticipants = table.Column<int>(type: "integer", nullable: true),
                    Commentaire = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.CheckConstraint("CK_Reservations_Heures", "\"HeureDebut\" BETWEEN 1 AND 29 AND \"HeureFin\" BETWEEN 1 AND 29 AND \"HeureFin\" > \"HeureDebut\"");
                    table.CheckConstraint("CK_Reservations_Participants", "\"NombreParticipants\" IS NULL OR \"NombreParticipants\" >= 1");
                    table.ForeignKey(
                        name: "FK_Reservations_Salles_SalleId",
                        column: x => x.SalleId,
                        principalTable: "Salles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CodePermanentReservateur",
                table: "Reservations",
                column: "CodePermanentReservateur");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_DateCreation",
                table: "Reservations",
                column: "DateCreation");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_HeureDebut",
                table: "Reservations",
                column: "HeureDebut");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_HeureFin",
                table: "Reservations",
                column: "HeureFin");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_SalleId",
                table: "Reservations",
                column: "SalleId");

            migrationBuilder.CreateIndex(
                name: "IX_Salles_Nom",
                table: "Salles",
                column: "Nom",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Salles");
        }
    }
}
