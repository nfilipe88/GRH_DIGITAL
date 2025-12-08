using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManager.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AtualizaAvaliacao_Com_Tenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InstituicaoId",
                table: "Avaliacoes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_InstituicaoId",
                table: "Avaliacoes",
                column: "InstituicaoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Avaliacoes_Instituicoes_InstituicaoId",
                table: "Avaliacoes",
                column: "InstituicaoId",
                principalTable: "Instituicoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Avaliacoes_Instituicoes_InstituicaoId",
                table: "Avaliacoes");

            migrationBuilder.DropIndex(
                name: "IX_Avaliacoes_InstituicaoId",
                table: "Avaliacoes");

            migrationBuilder.DropColumn(
                name: "InstituicaoId",
                table: "Avaliacoes");
        }
    }
}
