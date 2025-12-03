using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AvaliacoesController : ControllerBase
    {
        private readonly IAvaliacaoService _avaliacaoService;

        public AvaliacoesController(IAvaliacaoService avaliacaoService)
        {
            _avaliacaoService = avaliacaoService;
        }

        [HttpGet("equipa")]
        [Authorize(Roles = "GestorRH,GestorMaster")]
        public async Task<IActionResult> GetAvaliacoesEquipa()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var result = await _avaliacaoService.GetAvaliacoesEquipaAsync(email);
            return Ok(result);
        }

        [HttpPost("iniciar")]
        [Authorize(Roles = "GestorRH,GestorMaster")]
        public async Task<IActionResult> IniciarAvaliacao(int colaboradorId, int cicloId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            try
            {
                var result = await _avaliacaoService.IniciarAvaliacaoAsync(colaboradorId, cicloId, email);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPut("{id}/submeter")]
        public async Task<IActionResult> SubmeterAvaliacao(int id, [FromBody] SubmeterAvaliacaoRequest request)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            try
            {
                var result = await _avaliacaoService.SubmeterAvaliacaoAsync(id, request, email);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }
    }
}
