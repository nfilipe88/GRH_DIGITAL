using HRManager.WebAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly HRManagerDbContext _context;

        public DashboardController(HRManagerDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            // Esta lógica será expandida no futuro
            // Por agora, o GestorMaster vê tudo.

            var stats = new DashboardStatsDto
            {
                TotalInstituicoes = await _context.Instituicoes.CountAsync(),
                TotalColaboradores = await _context.Colaboradores.CountAsync(),
                TotalColaboradoresAtivos = await _context.Colaboradores
                                                .CountAsync(c => c.IsAtivo == true),
                TotalUtilizadores = await _context.Users.CountAsync(),
            };

            return Ok(stats);
        }
    }
}
