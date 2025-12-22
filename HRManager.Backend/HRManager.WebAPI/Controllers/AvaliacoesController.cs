using HRManager.WebAPI.Constants;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
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

        [HttpPost("ciclos")]
        [Authorize(Roles = RolesConstants.ApenasGestores)]
        public async Task<IActionResult> CriarCiclo([FromBody] CriarCicloRequest request)
        {
            var criado = await _avaliacaoService.CriarCicloAsync(request);
            return Created("", criado);
        }

        [HttpPost("competencias")]
        [Authorize(Roles = RolesConstants.ApenasGestores)]
        public async Task<IActionResult> CriarCompetencia([FromBody] CriarCompetenciaRequest request)
        {
            var criada = await _avaliacaoService.CriarCompetenciaAsync(request);
            return Created("", criada);
        }


        [HttpPost("iniciar")]
        [Authorize(Roles = RolesConstants.ApenasGestores)]
        public async Task<IActionResult> IniciarAvaliacao(Guid colaboradorId, Guid cicloId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var result = await _avaliacaoService.IniciarAvaliacaoAsync(colaboradorId, cicloId, email);
            return Ok(result);
        }

        [HttpGet("competencias")]
        public async Task<IActionResult> ListarCompetencias()
        {
            // Todos podem ver as competências (para saber o que está a ser avaliado)
            var lista = await _avaliacaoService.GetCompetenciasAsync();
            return Ok(lista);
        }

        [HttpGet("ciclos")]
        public async Task<IActionResult> ListarCiclos()
        {
            var lista = await _avaliacaoService.GetCiclosAsync();
            return Ok(lista);
        }

        // --- Processo de Avaliação ---

        [HttpGet("minhas")]
        public async Task<IActionResult> GetMinhasAvaliacoes()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var result = await _avaliacaoService.GetMinhasAvaliacoesAsync(email);
            return Ok(result);
        }

        [HttpGet("equipa")]
        [Authorize(Roles = RolesConstants.ApenasGestores)]
        public async Task<IActionResult> GetAvaliacoesEquipa()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var result = await _avaliacaoService.GetAvaliacoesEquipaAsync(email);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAvaliacaoPorId(Guid id)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            try
            {
                var result = await _avaliacaoService.GetAvaliacaoPorIdAsync(id, email);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Avaliação não encontrada.");
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPut("{id}/auto-avaliacao")]
        public async Task<IActionResult> RealizarAutoAvaliacao(Guid id, [FromBody] RealizarAutoAvaliacaoRequest request)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            // Middleware trata UnauthorizedAccessException se tentar editar a de outro
            var result = await _avaliacaoService.RealizarAutoAvaliacaoAsync(id, request, email);
            return Ok(result);
        }

        [HttpPut("{id}/avaliacao-gestor")]
        [Authorize(Roles = RolesConstants.ApenasGestores)]
        public async Task<IActionResult> SubmeterAvaliacaoGestor(Guid id, [FromBody] RealizarAvaliacaoGestorRequest request)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var result = await _avaliacaoService.RealizarAvaliacaoGestorAsync(id, request, email);
            return Ok(result);
        }
    }
}
