using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HRManager.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTabelasPerfilProfissional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CertificacoesProfissionais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ColaboradorId = table.Column<int>(type: "integer", nullable: false),
                    NomeCertificacao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntidadeEmissora = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DataEmissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataValidade = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CaminhoDocumento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificacoesProfissionais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CertificacoesProfissionais_Colaboradores_ColaboradorId",
                        column: x => x.ColaboradorId,
                        principalTable: "Colaboradores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HabilitacoesLiterarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ColaboradorId = table.Column<int>(type: "integer", nullable: false),
                    Grau = table.Column<int>(type: "integer", nullable: false),
                    Curso = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InstituicaoEnsino = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DataConclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CaminhoDocumento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HabilitacoesLiterarias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HabilitacoesLiterarias_Colaboradores_ColaboradorId",
                        column: x => x.ColaboradorId,
                        principalTable: "Colaboradores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CertificacoesProfissionais_ColaboradorId",
                table: "CertificacoesProfissionais",
                column: "ColaboradorId");

            migrationBuilder.CreateIndex(
                name: "IX_HabilitacoesLiterarias_ColaboradorId",
                table: "HabilitacoesLiterarias",
                column: "ColaboradorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CertificacoesProfissionais");

            migrationBuilder.DropTable(
                name: "HabilitacoesLiterarias");
        }
    }
}
