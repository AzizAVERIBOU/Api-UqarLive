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

            migrationBuilder.AlterColumn<string>(
                name: "Telephone",
                table: "Utilisateurs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """ALTER TABLE "Utilisateurs" ALTER COLUMN "Telephone" TYPE double precision USING "Telephone"::double precision;""");

            migrationBuilder.AlterColumn<double>(
                name: "Telephone",
                table: "Utilisateurs",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);
        }
    }
}
