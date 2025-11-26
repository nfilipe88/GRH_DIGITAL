using ClosedXML.Excel;
using HRManager.Application.Interfaces;
using HRManager.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "GestorMaster, GestorRH")] // Apenas gestores podem exportar
    public class RelatoriosController : ControllerBase
    {
        private readonly HRManagerDbContext _context;
        private readonly ITenantService _tenantService;

        public RelatoriosController(HRManagerDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        [HttpGet("ausencias")]
        public async Task<IActionResult> ExportarAusencias([FromQuery] int mes, [FromQuery] int ano)
        {
            // 1. Construir a Query Base
            var query = _context.Ausencias
                .Include(a => a.Colaborador)
                .AsQueryable();

            // 2. Filtrar por Data (ausências que coincidam com o mês/ano solicitado)
            // Lógica: O início ou o fim da ausência cai neste mês?
            // Simplificação: Vamos buscar todas as que começam neste mês e ano
            query = query.Where(a => a.DataInicio.Month == mes && a.DataInicio.Year == ano);

            // 3. Filtrar por Segurança (Multi-tenant)
            if (User.IsInRole("GestorRH"))
            {
                var tenantId = _tenantService.GetTenantId();
                if (tenantId.HasValue)
                {
                    query = query.Where(a => a.Colaborador.InstituicaoId == tenantId.Value);
                }
            }

            // 4. Buscar dados
            var dados = await query
                .OrderBy(a => a.DataInicio)
                .ToListAsync();

            // 5. Gerar o Excel usando ClosedXML
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Ausências");

                // Cabeçalhos
                worksheet.Cell(1, 1).Value = "Colaborador";
                worksheet.Cell(1, 2).Value = "NIF";
                worksheet.Cell(1, 3).Value = "Tipo";
                worksheet.Cell(1, 4).Value = "Início";
                worksheet.Cell(1, 5).Value = "Fim";
                worksheet.Cell(1, 6).Value = "Duração (Dias)";
                worksheet.Cell(1, 7).Value = "Estado";
                worksheet.Cell(1, 8).Value = "Motivo";

                // Estilo Cabeçalho
                var headerRange = worksheet.Range("A1:H1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Preencher Dados
                int row = 2;
                foreach (var item in dados)
                {
                    worksheet.Cell(row, 1).Value = item.Colaborador.NomeCompleto;
                    worksheet.Cell(row, 2).Value = item.Colaborador.NIF;
                    worksheet.Cell(row, 3).Value = item.Tipo.ToString();
                    worksheet.Cell(row, 4).Value = item.DataInicio; // ClosedXML formata datas automaticamente
                    worksheet.Cell(row, 5).Value = item.DataFim;
                    worksheet.Cell(row, 6).Value = (item.DataFim - item.DataInicio).Days + 1;
                    worksheet.Cell(row, 7).Value = item.Estado.ToString();
                    worksheet.Cell(row, 8).Value = item.Motivo;

                    // Colorir estado (Opcional)
                    if (item.Estado == EstadoAusencia.Aprovada)
                        worksheet.Cell(row, 7).Style.Font.FontColor = XLColor.Green;
                    if (item.Estado == EstadoAusencia.Pendente)
                        worksheet.Cell(row, 7).Style.Font.FontColor = XLColor.Orange;

                    row++;
                }

                // Ajustar largura das colunas
                worksheet.Columns().AdjustToContents();

                // 6. Preparar o ficheiro para download
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Ausencias_{mes}_{ano}.xlsx");
                }
            }
        }
    }
}
