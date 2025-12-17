using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> GetInstituicaoById(Guid id)
        {
            var instituicao = await _instituicaoService.GetByIdAsync(id);
            return Ok(instituicao);
        }

        [HttpPost]
        public async Task<IActionResult> CriarInstituicao([FromBody] CriarInstituicaoRequest request)
        {
            var novaInstituicao = await _instituicaoService.CreateAsync(request);
            return CreatedAtAction(nameof(GetInstituicaoById), new { id = novaInstituicao.Id }, novaInstituicao);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarInstituicao(Guid id, [FromBody] AtualizarInstituicaoRequest request)
        {
            var atualizada = await _instituicaoService.UpdateAsync(id, request);
            return Ok(atualizada);
        }
    }
}
