using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HRManager.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requer login para tudo
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
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
            var isGestorRH = User.IsInRole("GestorRH");
            var isGestorMaster = User.IsInRole("GestorMaster");

            var result = await _ausenciaService.GetAusenciasAsync(email, isGestorRH, isGestorMaster);
            return Ok(result);
        }

        [HttpGet("saldo")]
        public async Task<IActionResult> GetMeuSaldo()
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
                var result = await _ausenciaService.GetSaldoAsync(email);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SolicitarAusencia([FromForm] CriarAusenciaRequest request)
        {
            // O FluentValidation (configurado no Program.cs) já validou os campos básicos aqui.
            // Se chegou aqui, DataInicio e Tipo são válidos.

            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");

            try
            {
                await _ausenciaService.SolicitarAusenciaAsync(email, request);
                return StatusCode(201, new { message = "Pedido de ausência submetido com sucesso." });
            }
            catch (ValidationException ex) // Erros de Negócio (ex: Saldo, Conflito)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex) // Erros inesperados
            {
                return BadRequest(new { message = "Erro ao processar pedido: " + ex.Message });
            }
        }

        [HttpPut("{id}/responder")]
        [Authorize(Roles = "GestorMaster, GestorRH")]
        public async Task<IActionResult> ResponderAusencia(int id, [FromBody] ResponderAusenciaRequest request)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var isGestorRH = User.IsInRole("GestorRH");

            try
            {
                await _ausenciaService.ResponderAusenciaAsync(id, request, email, isGestorRH);
                string acao = request.Aprovado ? "aprovado" : "rejeitado";
                return Ok(new { message = $"Pedido {acao} com sucesso." });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Pedido não encontrado." });
            }
        }
    }
}
