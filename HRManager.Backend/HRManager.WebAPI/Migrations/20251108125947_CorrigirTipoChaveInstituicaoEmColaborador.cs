using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManager.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class CorrigirTipoChaveInstituicaoEmColaborador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "Colaboradores",
                newName: "NomeCompleto");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Colaboradores",
                newName: "NIF");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAdmissao",
                table: "Colaboradores",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EmailPessoal",
                table: "Colaboradores",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "InstituicaoId",
                table: "Colaboradores",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "NumeroAgente",
                table: "Colaboradores",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Instituicoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IdentificadorUnico = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsAtiva = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instituicoes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Colaboradores_InstituicaoId",
                table: "Colaboradores",
                column: "InstituicaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Colaboradores_NIF_InstituicaoId",
                table: "Colaboradores",
                columns: new[] { "NIF", "InstituicaoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instituicoes_IdentificadorUnico",
                table: "Instituicoes",
                column: "IdentificadorUnico",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Colaboradores_Instituicoes_InstituicaoId",
                table: "Colaboradores",
                column: "InstituicaoId",
                principalTable: "Instituicoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Colaboradores_Instituicoes_InstituicaoId",
                table: "Colaboradores");

            migrationBuilder.DropTable(
                name: "Instituicoes");

            migrationBuilder.DropIndex(
                name: "IX_Colaboradores_InstituicaoId",
                table: "Colaboradores");

            migrationBuilder.DropIndex(
                name: "IX_Colaboradores_NIF_InstituicaoId",
                table: "Colaboradores");

            migrationBuilder.DropColumn(
                name: "DataAdmissao",
                table: "Colaboradores");

            migrationBuilder.DropColumn(
                name: "EmailPessoal",
                table: "Colaboradores");

            migrationBuilder.DropColumn(
                name: "InstituicaoId",
                table: "Colaboradores");

            migrationBuilder.DropColumn(
                name: "NumeroAgente",
                table: "Colaboradores");

            migrationBuilder.RenameColumn(
                name: "NomeCompleto",
                table: "Colaboradores",
                newName: "Nome");

            migrationBuilder.RenameColumn(
                name: "NIF",
                table: "Colaboradores",
                newName: "Email");
        }
    }
}
