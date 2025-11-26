using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManager.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarMoradaIBAN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "Colaboradores",
                type: "character varying(34)",
                maxLength: 34,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Morada",
                table: "Colaboradores",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IBAN",
                table: "Colaboradores");

            migrationBuilder.DropColumn(
                name: "Morada",
                table: "Colaboradores");
        }
    }
}
