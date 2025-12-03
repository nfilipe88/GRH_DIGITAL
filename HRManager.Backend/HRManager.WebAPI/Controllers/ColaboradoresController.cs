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

        // ---
        // MÉTODO GET (Listar) - VERSÃO OTIMIZADA COM DTO
        // Mapeado para: GET api/Colaboradores
        // ---
        [HttpGet]
        // Adicione [FromQuery] Guid? instituicaoId
        public async Task<IActionResult> GetColaboradores([FromQuery] Guid? instituicaoId = null)
        {
            // Passa o parâmetro (que pode ser null) para o serviço
            var lista = await _colaboradorService.GetAllAsync(instituicaoId);
            return Ok(lista);
        }

        // ---
        // MÉTODO POST (Cadastrar)
        // Mapeado para: POST api/Colaboradores
        // ---
        [HttpPost]
        public async Task<IActionResult> CriarColaborador([FromBody] CriarColaboradorRequest request)
        {
            // 1. Verificar se é GestorRH e forçar o ID do Token
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole == "GestorRH")
            {
                var tenantIdClaim = User.FindFirstValue("InstituicaoId");
                if (string.IsNullOrEmpty(tenantIdClaim))
                {
                    return Unauthorized(new { message = "Token inválido: Instituição não encontrada." });
                }
                // Sobrescreve o que veio do formulário para garantir segurança
                request.InstituicaoId = Guid.Parse(tenantIdClaim);
            }
            // Validação automática do FluentValidation ocorre aqui antes de entrar no método
            try
            {
                var criado = await _colaboradorService.CreateAsync(request);
                return CreatedAtAction(nameof(GetColaboradorPorId), new { id = criado.Id }, criado);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ---
        // NOVO MÉTODO GET (Buscar por ID)
        // Mapeado para: GET api/Colaboradores/{id}
        // ---
        [HttpGet("{id}")]
        public async Task<IActionResult> GetColaboradorPorId(int id)
        {
            // Vamos buscar o colaborador pela sua chave primária (Id)
            // Usamos o AsNoTracking() porque apenas queremos ler os dados, não os vamos modificar aqui
            var colaborador = await _colaboradorService.GetByIdAsync(id);
            if (colaborador == null) return NotFound(new { message = "Colaborador não encontrado." });
            return Ok(colaborador);
        }

        // ---
        // NOVO MÉTODO PUT (Atualizar)
        // Mapeado para: PUT api/Colaboradores/{id}
        // ---
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarColaborador(int id, [FromBody] AtualizarDadosPessoaisRequest request)
        {
            try
            {
                var atualizado = await _colaboradorService.UpdateAsync(id, request);
                return Ok(atualizado);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Colaborador não encontrado." });
            }
        }

        // ---
        // MÉTODO ATUALIZADO: De DELETE para PATCH (Soft Delete)
        // Mapeado para: PATCH api/Colaboradores/{id}/estado
        // ---
        // [HttpDelete("{id}")] <-- REMOVA O ANTIGO MÉTODO DELETE
        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> AtualizarEstadoColaborador(int id, [FromBody] AtualizarEstadoRequest request)
        {
            // Reutilizamos o DTO 'AtualizarEstadoRequest' que já existe

            try
            {
                await _colaboradorService.ToggleAtivoAsync(id);
                return Ok(new { message = "Estado alterado com sucesso." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // ---
        // MÉTODO DELETE (Eliminar)
        // Mapeado para: DELETE api/Colaboradores/{id}
        // ---
        //[HttpDelete("{id}")]
        //[Authorize(Roles = "GestorMaster")]
        //public async Task<IActionResult> DeletarColaborador(int id)
        //{
        //    try
        //    {
        //        await _colaboradorService.DeleteAsync(id);
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }

        //    return Ok(new { message = $"Colaborador '{colaborador.NomeCompleto}' eliminado com sucesso." });
        //}
    }
}
