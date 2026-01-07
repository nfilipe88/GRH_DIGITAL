using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HRManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            // Obtém o email do utilizador logado através do Token
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var stats = await _dashboardService.GetDashboardStatsAsync(email, roles);
            return Ok(stats);
        }

        //[HttpGet("stats")]
        //public async Task<IActionResult> GetStats([FromQuery] Guid? instituicaoId = null)
        //{
        //    // O serviço decide se pode usar o instituicaoId (Master) ou ignora (Tenant normal)
        //    var stats = await _dashboardService.GetStatsAsync(instituicaoId);
        //    return Ok(stats);
        //}

        //[HttpGet("stats")]
        //public async Task<IActionResult> GetStats([FromQuery] Guid? instituicaoId = null)
        //{
        //    // Obter userId e roles do usuário autenticado
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    var roles = User.Claims
        //        .Where(c => c.Type == ClaimTypes.Role)
        //        .Select(c => c.Value);

        //    // Se temos userId e roles, use o método específico
        //    if (!string.IsNullOrEmpty(userId))
        //    {
        //        var stats = await _dashboardService.GetStatsAsync(Guid.Parse(userId), roles);
        //        return Ok(stats);
        //    }

        //    // Fallback: use o método sem parâmetros
        //    var fallbackStats = await _dashboardService.GetStatsAsync();
        //    return Ok(fallbackStats);
        //}
    }
}
