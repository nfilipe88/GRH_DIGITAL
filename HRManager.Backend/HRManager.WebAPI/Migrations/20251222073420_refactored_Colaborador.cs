using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManager.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class refactored_Colaborador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Colaboradores_Colaboradores_GestorId",
                table: "Colaboradores");

            migrationBuilder.AlterColumn<string>(
                name: "Departamento",
                table: "Colaboradores",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Colaboradores_Colaboradores_GestorId",
                table: "Colaboradores",
                column: "GestorId",
                principalTable: "Colaboradores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Colaboradores_Colaboradores_GestorId",
                table: "Colaboradores");

            migrationBuilder.AlterColumn<string>(
                name: "Departamento",
                table: "Colaboradores",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_Colaboradores_Colaboradores_GestorId",
                table: "Colaboradores",
                column: "GestorId",
                principalTable: "Colaboradores",
                principalColumn: "Id");
        }
    }
}
