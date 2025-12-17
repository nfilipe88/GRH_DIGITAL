using FluentValidation;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "GestorMaster, GestorRH")]
    public class ColaboradoresController : ControllerBase
    {
        private readonly IColaboradorService _colaboradorService;

        public ColaboradoresController(IColaboradorService colaboradorService)
        {
            _colaboradorService = colaboradorService;
        }

        // APENAS ESTE MÉTODO GET
        [HttpGet]
        public async Task<IActionResult> GetColaboradores([FromQuery] Guid? instituicaoId = null)
        {
            var lista = await _colaboradorService.GetAllAsync();
            return Ok(lista);
        }

        [HttpPost]
        public async Task<IActionResult> CriarColaborador([FromBody] CriarColaboradorRequest request)
        {
            var novoColaborador = await _colaboradorService.CreateAsync(request);
            return CreatedAtAction(nameof(GetColaboradorPorId), new { id = novoColaborador.Id }, novoColaborador);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetColaboradorPorId(Guid id)
        {
            var colaborador = await _colaboradorService.GetByIdAsync(id);
            return Ok(colaborador);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarColaborador(Guid id, [FromBody] AtualizarDadosPessoaisRequest request)
        {
            var atualizado = await _colaboradorService.UpdateAsync(id, request);
            return Ok(atualizado);
        }

        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> AtualizarEstadoColaborador(Guid id, [FromBody] AtualizarEstadoRequest request)
        {
            await _colaboradorService.ToggleAtivoAsync(id);
            return NoContent();
        }

        // ---
        // MÉTODO DELETE (Eliminar)
        // Mapeado para: DELETE api/Colaboradores/{id}
        // ---
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeletarColaborador(int id)
        //{
        //    await _colaboradorService.DeleteAsync(id);
        //    return NoContent();
        //}
    }
}
