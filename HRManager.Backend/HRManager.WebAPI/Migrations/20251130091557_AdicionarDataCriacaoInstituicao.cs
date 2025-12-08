using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManager.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDataCriacaoInstituicao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataCriacao",
                table: "Instituicoes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EmailContato",
                table: "Instituicoes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Endereco",
                table: "Instituicoes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NIF",
                table: "Instituicoes",
                type: "integer",
                maxLength: 15,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Telemovel",
                table: "Instituicoes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataCriacao",
                table: "Instituicoes");

            migrationBuilder.DropColumn(
                name: "EmailContato",
                table: "Instituicoes");

            migrationBuilder.DropColumn(
                name: "Endereco",
                table: "Instituicoes");

            migrationBuilder.DropColumn(
                name: "NIF",
                table: "Instituicoes");

            migrationBuilder.DropColumn(
                name: "Telemovel",
                table: "Instituicoes");
        }
    }
}
