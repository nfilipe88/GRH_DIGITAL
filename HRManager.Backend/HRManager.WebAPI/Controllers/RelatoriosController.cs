using ClosedXML.Excel;
using HRManager.Application.Interfaces;
using HRManager.WebAPI.Domain.enums;
using HRManager.WebAPI.Domain.Interfaces;
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
        private readonly IAusenciaService _ausenciaService;

        // Injetamos o Serviço em vez do Contexto
        public RelatoriosController(IAusenciaService ausenciaService)
        {
            _ausenciaService = ausenciaService;
        }

        [HttpGet("ausencias")]
        public async Task<IActionResult> ExportarAusencias([FromQuery] int mes, [FromQuery] int ano)
        {
            // O Controller apenas delega a tarefa
            var ficheiroBytes = await _ausenciaService.DownloadRelatorioExcelAsync(mes, ano);

            // E retorna o ficheiro
            return File(
                ficheiroBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Ausencias_{mes}_{ano}.xlsx");
        }
    }
}
