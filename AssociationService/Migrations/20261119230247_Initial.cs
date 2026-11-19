using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AssociationService.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "Associations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Categorie = table.Column<int>(type: "integer", nullable: false),
                    Faculte = table.Column<int>(type: "integer", nullable: false),
                    CodePermanentCreateur = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Objectifs = table.Column<List<string>>(type: "text[]", nullable: false),
                    Activites = table.Column<List<string>>(type: "text[]", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Statut = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Associations", x => x.Id);
                    table.CheckConstraint("CK_Associations_Categorie", "\"Categorie\" BETWEEN 0 AND 9");
                    table.CheckConstraint("CK_Associations_Faculte", "\"Faculte\" BETWEEN 0 AND 7");
                    table.CheckConstraint("CK_Associations_Statut", "\"Statut\" BETWEEN 0 AND 3");
                });

            migrationBuilder.CreateTable(
                name: "DemandesAdhesion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssociationId = table.Column<int>(type: "integer", nullable: false),
                    CodePermanent = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    DateDemande = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Motivation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Statut = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandesAdhesion", x => x.Id);
                    table.CheckConstraint("CK_DemandesAdhesion_Role", "\"Role\" BETWEEN 0 AND 4");
                    table.CheckConstraint("CK_DemandesAdhesion_Statut", "\"Statut\" BETWEEN 0 AND 2");
                    table.ForeignKey(
                        name: "FK_DemandesAdhesion_Associations_AssociationId",
                        column: x => x.AssociationId,
                        principalTable: "Associations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Evenements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssociationId = table.Column<int>(type: "integer", nullable: false),
                    Titre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DateDebut = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Lieu_NumeroCivique = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Lieu_Rue = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Lieu_Appartement = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Lieu_Ville = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Lieu_Province = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Lieu_CodePostal = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CapaciteMax = table.Column<int>(type: "integer", nullable: true),
                    Participants = table.Column<List<string>>(type: "text[]", nullable: false),
                    Statut = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evenements", x => x.Id);
                    table.CheckConstraint("CK_Evenements_Capacite", "\"CapaciteMax\" IS NULL OR \"CapaciteMax\" > 0");
                    table.CheckConstraint("CK_Evenements_Dates", "\"DateFin\" > \"DateDebut\"");
                    table.CheckConstraint("CK_Evenements_Statut", "\"Statut\" BETWEEN 0 AND 3");
                    table.ForeignKey(
                        name: "FK_Evenements_Associations_AssociationId",
                        column: x => x.AssociationId,
                        principalTable: "Associations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Membres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssociationId = table.Column<int>(type: "integer", nullable: false),
                    CodePermanent = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    DateAdhesion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Statut = table.Column<int>(type: "integer", nullable: false),
                    Responsabilites = table.Column<List<string>>(type: "text[]", nullable: false),
                    Associations = table.Column<List<string>>(type: "text[]", nullable: false),
                    RoleUtilisateur = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Membres", x => x.Id);
                    table.CheckConstraint("CK_Membres_Role", "\"Role\" BETWEEN 0 AND 4");
                    table.CheckConstraint("CK_Membres_RoleUtilisateur", "\"RoleUtilisateur\" BETWEEN 0 AND 2");
                    table.CheckConstraint("CK_Membres_Statut", "\"Statut\" BETWEEN 0 AND 3");
                    table.ForeignKey(
                        name: "FK_Membres_Associations_AssociationId",
                        column: x => x.AssociationId,
                        principalTable: "Associations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Associations_Nom",
                table: "Associations",
                column: "Nom",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DemandesAdhesion_AssociationId_CodePermanent",
                table: "DemandesAdhesion",
                columns: new[] { "AssociationId", "CodePermanent" },
                unique: true,
                filter: "\"Statut\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Evenements_AssociationId",
                table: "Evenements",
                column: "AssociationId");

            migrationBuilder.CreateIndex(
                name: "IX_Membres_AssociationId_CodePermanent",
                table: "Membres",
                columns: new[] { "AssociationId", "CodePermanent" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DemandesAdhesion");

            migrationBuilder.DropTable(
                name: "Evenements");

            migrationBuilder.DropTable(
                name: "Membres");

            migrationBuilder.DropTable(
                name: "Associations");
        }
    }
}
