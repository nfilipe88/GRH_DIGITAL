using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManager.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class CorrigirTipoTelefoneInstituicao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Converter valores numéricos válidos, outros para NULL
            migrationBuilder.Sql(@"
                ALTER TABLE ""Instituicoes"" 
                ALTER COLUMN ""Telemovel"" TYPE integer 
                USING CASE 
                    WHEN ""Telemovel"" ~ '^[0-9]+$' THEN ""Telemovel""::integer 
                    ELSE NULL 
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Converter de volta para string (conversão sempre válida)
            migrationBuilder.Sql(@"
                ALTER TABLE ""Instituicoes"" 
                ALTER COLUMN ""Telemovel"" TYPE text 
                USING ""Telemovel""::text
            ");
        }
    }
}
