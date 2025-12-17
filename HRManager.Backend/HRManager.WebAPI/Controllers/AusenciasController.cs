using HRManager.WebAPI.Constants;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HRManager.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AusenciasController : ControllerBase
    {
        private readonly IAusenciaService _ausenciaService;

        // Injeção de Dependência limpa: Sem DbContext aqui!
        public AusenciasController(IAusenciaService ausenciaService)
        {
            _ausenciaService = ausenciaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAusencias()
        {
            // O serviço já sabe lidar com as permissões se passarmos o contexto correto
            // Usamos as extensões para limpar a leitura dos Claims
            var email = User.FindFirst("email")?.Value; // ou User.Identity.Name
            var isGestor = User.IsRole(RolesConstants.GestorRH) || User.IsRole(RolesConstants.GestorMaster);

            var result = await _ausenciaService.GetAusenciasAsync(email, isGestor, User.IsRole(RolesConstants.GestorMaster));
            return Ok(result);
        }

        [HttpGet("saldo")]
        public async Task<IActionResult> GetMeuSaldo()
        {
            // 1. Tenta obter pelo NameIdentifier (Padrão .NET)
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 2. Fallback: Se falhar, tenta pelo claim "id" (comum em alguns JWTs custom) ou "sub"
            if (string.IsNullOrEmpty(userId))
            {
                userId = User.FindFirst("id")?.Value ?? User.FindFirst("sub")?.Value;
            }

            // 3. Se ainda assim for nulo, bloqueia aqui (não chama o serviço)
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "Não foi possível identificar o utilizador no token." });
            }

            try
            {
                var result = await _ausenciaService.GetSaldoAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log opcional aqui
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SolicitarAusencia([FromForm] CriarAusenciaRequest request)
        {
            var email = User.FindFirst("email")?.Value;
            await _ausenciaService.SolicitarAusenciaAsync(email, request);
            return StatusCode(201, new { message = "Pedido de ausência submetido com sucesso." });
        }

        [HttpPut("{id}/responder")]
        [Authorize(Roles = RolesConstants.ApenasGestores)] // Uso da constante
        public async Task<IActionResult> ResponderAusencia(Guid id, [FromBody] ResponderAusenciaRequest request)
        {
            var email = User.FindFirst("email")?.Value;
            var isGestorRH = User.IsRole(RolesConstants.GestorRH);

            await _ausenciaService.ResponderAusenciaAsync(id, request, email, isGestorRH);

            string acao = request.Aprovado ? "aprovado" : "rejeitado";
            return Ok(new { message = $"Pedido {acao} com sucesso." });
        }
    }
}
