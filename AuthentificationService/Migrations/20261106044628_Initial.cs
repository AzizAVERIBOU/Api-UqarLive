using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AuthentificationService.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    CodePermanent = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Prenom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MotDePasseHash = table.Column<byte[]>(type: "bytea", nullable: false),
                    MotDePasseSalt = table.Column<byte[]>(type: "bytea", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Programme = table.Column<string>(type: "text", nullable: false),
                    Faculte = table.Column<string>(type: "text", nullable: false),
                    AnneeDebut = table.Column<int>(type: "integer", nullable: false),
                    AnneeFin = table.Column<int>(type: "integer", nullable: false),
                    Telephone = table.Column<double>(type: "double precision", nullable: false),
                    DateInscription = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DerniereConnexion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    EstActif = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.CodePermanent);
                });

            migrationBuilder.CreateTable(
                name: "Adresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroCivique = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Rue = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Appartement = table.Column<string>(type: "text", nullable: true),
                    Ville = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Province = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CodePostal = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CodePermanentUtilisateur = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Adresses_Utilisateurs_CodePermanentUtilisateur",
                        column: x => x.CodePermanentUtilisateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "CodePermanent");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Adresses_CodePermanentUtilisateur",
                table: "Adresses",
                column: "CodePermanentUtilisateur");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_Email",
                table: "Utilisateurs",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Adresses");

            migrationBuilder.DropTable(
                name: "Utilisateurs");
        }
    }
}
