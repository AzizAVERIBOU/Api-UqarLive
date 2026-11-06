using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthentificationService.Migrations
{
    /// <inheritdoc />
    public partial class TelephoneEnTexte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """ALTER TABLE "Utilisateurs" ALTER COLUMN "Telephone" TYPE character varying(20) USING LEFT("Telephone"::text, 20);""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """ALTER TABLE "Utilisateurs" ALTER COLUMN "Telephone" TYPE double precision USING "Telephone"::double precision;""");
        }
    }
}
