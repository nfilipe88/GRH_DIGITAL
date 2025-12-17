using HRManager.Application.Interfaces;
using HRManager.WebAPI.Constants;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace HRManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DeclaracoesController : ControllerBase
    {
        private readonly IDeclaracaoService _declaracaoService;

        public DeclaracoesController(IDeclaracaoService declaracaoService)
        {
            _declaracaoService = declaracaoService;
        }

        // 1. Colaborador solicita
        [HttpPost("solicitar")]
        public async Task<IActionResult> Solicitar([FromBody] CriarPedidoDeclaracaoRequest request)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var result = await _declaracaoService.CriarPedidoAsync(request, email);
            // Retorna 201 (Created) ou 200 (OK)
            return Ok(result);
        }

        // 2. Colaborador vê as suas
        [HttpGet("minhas")]
        public async Task<IActionResult> GetMinhas()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var result = await _declaracaoService.GetMeusPedidosAsync(email);
            return Ok(result);
        }

        // 3. RH vê pendentes (Só Gestores)
        [HttpGet("pendentes")]
        [Authorize(Roles = RolesConstants.ApenasGestores)]
        public async Task<IActionResult> GetPendentes()
        {
            var result = await _declaracaoService.GetPedidosPendentesAsync();
            return Ok(result);
        }

        // 4. RH gera o PDF (Só Gestores)
        [HttpPut("{id}/gerar")]
        [Authorize(Roles = RolesConstants.ApenasGestores)]
        public async Task<IActionResult> GerarDeclaracao(Guid id)
        {
            var emailGestor = User.FindFirstValue(ClaimTypes.Email);

            // O serviço gera os bytes e notifica o colaborador
            var pdfBytes = await _declaracaoService.GerarDeclaracaoPdfAsync(id, emailGestor);

            return File(pdfBytes, "application/pdf", $"Declaracao_{id}.pdf");
        }

        // 5. NOVO: RH Rejeita ou altera estado sem gerar (Opcional para o futuro)
        [HttpPatch("{id}/estado")]
        [Authorize(Roles = RolesConstants.ApenasGestores)]
        public async Task<IActionResult> MudarEstado(Guid id, [FromBody] bool aprovado)
        {
            var emailGestor = User.FindFirstValue(ClaimTypes.Email);
            await _declaracaoService.AtualizarEstadoPedidoAsync(id, aprovado, emailGestor);
            return NoContent();
        }
    }
}
