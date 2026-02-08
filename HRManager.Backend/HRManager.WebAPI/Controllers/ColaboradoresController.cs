using FluentValidation;
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
    [Authorize(Roles = RolesConstants.GestorMaster + "," + RolesConstants.GestorRH)]
    public class ColaboradoresController : ControllerBase
    {
        private readonly IColaboradorService _colaboradorService;

        public ColaboradoresController(IColaboradorService colaboradorService)
        {
            _colaboradorService = colaboradorService;
        }

        
        [HttpGet]
        [Authorize(Roles = RolesConstants.GestorMaster)]
        public async Task<IActionResult> GetColaboradores()
        {
            var lista = await _colaboradorService.GetAllAsync();
            return Ok(lista);
        }

        [HttpGet("instituicao/{instituicaoId:guid}")]
        public async Task<IActionResult> GetColaboradoresPorInstituicao(Guid instituicaoId)
        {
            var lista = await _colaboradorService.GetAllByInstituicaoAsync(instituicaoId);
            return Ok(lista);
        }
        
        // --- NOVO ENDPOINT (Colocar aqui para evitar conflito de rotas) ---
        [HttpGet("cargos")]
        public async Task<IActionResult> GetCargos()
        {
            var cargos = await _colaboradorService.GetCargosAsync();
            return Ok(cargos);
        }
        // -----------------------------------------------------------------

        [HttpGet("{id}")]
        public async Task<IActionResult> GetColaboradorPorId(Guid id)
        {
            var colaborador = await _colaboradorService.GetByIdAsync(id);
            return Ok(colaborador);
        }

        [HttpPost]
        public async Task<IActionResult> CriarColaborador([FromBody] CriarColaboradorRequest request)
        {
            var novoColaborador = await _colaboradorService.CreateAsync(request);
            return CreatedAtAction(nameof(GetColaboradorPorId), new { id = novoColaborador.Id }, novoColaborador);
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

        [HttpPut("transferir-equipa")]
        [Authorize(Roles = "GestorRH,GestorMaster")]
        public async Task<IActionResult> TransferirEquipa([FromQuery] Guid gestorAntigoId, [FromQuery] Guid gestorNovoId)
        {
            try
            {
                await _colaboradorService.TransferirEquipaAsync(gestorAntigoId, gestorNovoId);
                return Ok(new { message = "Equipa transferida com sucesso." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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
