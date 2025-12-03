using FluentValidation;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "GestorMaster")] // Segurança Crítica: Apenas o "Dono" do SaaS deve mexer aqui
    public class InstituicoesController : ControllerBase
    {
        private readonly IInstituicaoService _instituicaoService;

        public InstituicoesController(IInstituicaoService instituicaoService)
        {
            _instituicaoService = instituicaoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetInstituicoes()
        {
            var lista = await _instituicaoService.GetAllAsync();
            return Ok(lista);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInstituicao(Guid id)
        {
            var instituicao = await _instituicaoService.GetByIdAsync(id);
            if (instituicao == null) return NotFound();
            return Ok(instituicao);
        }

        [HttpPost]
        public async Task<IActionResult> CriarInstituicao([FromBody] CriarInstituicaoRequest request)
        {
            try
            {
                var criada = await _instituicaoService.CreateAsync(request);
                return CreatedAtAction(nameof(GetInstituicao), new { id = criada.Id }, criada);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarInstituicao(Guid id, [FromBody] AtualizarInstituicaoRequest request)
        {
            try
            {
                var atualizada = await _instituicaoService.UpdateAsync(id, request);
                return Ok(atualizada);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
