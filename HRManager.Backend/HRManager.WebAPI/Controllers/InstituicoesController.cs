using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstituicoesController : ControllerBase
    {
        private readonly HRManagerDbContext _context;

        // "Injeção de Dependência": Pedimos ao .NET para nos dar o DbContext
        public InstituicoesController(HRManagerDbContext context)
        {
            _context = context;
        }

        // ---
        // MÉTODO GET (Listar)
        // Mapeado para: GET api/Instituicoes
        // ---
        [HttpGet]
        public async Task<IActionResult> GetInstituicoes()
         {
            // Vai à base de dados, busca todas as instituições e envia como resposta
            var instituicoes = await _context.Instituicoes.ToListAsync();
            return Ok(instituicoes); // Retorna um status 200 OK com os dados
        }

        // ---
        // MÉTODO POST (Criar)
        // Mapeado para: POST api/Instituicoes
        // ---
        [HttpPost]
        public async Task<IActionResult> CriarInstituicao([FromBody] CriarInstituicaoRequest request)
        {
            // O [FromBody] diz à API para ler os dados do corpo do pedido
            // O .NET valida automaticamente o DTO (graças ao [ApiController])

            //[cite_start]// 1. Verificar se o Slug já existe (baseado na RN-01.1 [cite: 73])
            if (await _context.Instituicoes.AnyAsync(i => i.IdentificadorUnico == request.IdentificadorUnico))
            {
                // Retorna um erro 400 Bad Request se o slug já estiver em uso
                return BadRequest(new { message = $"O Identificador (Slug) '{request.IdentificadorUnico}' já está em uso." });
            }

            // 2. Mapear o DTO (pedido) para o Modelo (base de dados)
            var novaInstituicao = new Instituicao
            {
                Nome = request.Nome,
                IdentificadorUnico = request.IdentificadorUnico,
                IsAtiva = true // Por defeito, nova instituição começa ativa
            };

            // 3. Adicionar ao DbContext e Salvar
            _context.Instituicoes.Add(novaInstituicao);
            await _context.SaveChangesAsync(); // O "Commit" para a base de dados

            // 4. Retornar uma resposta 201 Created
            // É uma boa prática retornar o objeto recém-criado
            return CreatedAtAction(nameof(GetInstituicoes), new { id = novaInstituicao.Id }, novaInstituicao);
        }

        // ---
        // MÉTODO PUT (Editar)
        // Mapeado para: PUT api/Instituicoes/{id}
        // ---
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarInstituicao(Guid id, [FromBody] AtualizarInstituicaoRequest request)
        {
            var instituicao = await _context.Instituicoes.FindAsync(id);

            if (instituicao == null)
            {
                return NotFound(new { message = "Instituição não encontrada." });
            }

            //[cite_start]// 1. Verificar se o novo IdentificadorUnico já existe noutra instituição (RN-01.1) 
            if (await _context.Instituicoes.AnyAsync(i => i.IdentificadorUnico == request.IdentificadorUnico && i.Id != id))
            {
                return BadRequest(new { message = $"O Identificador (Slug) '{request.IdentificadorUnico}' já está em uso." });
            }

            // 2. Atualizar as propriedades
            instituicao.Nome = request.Nome;
            instituicao.IdentificadorUnico = request.IdentificadorUnico; //

            // 3. Salvar
            _context.Instituicoes.Update(instituicao);
            await _context.SaveChangesAsync();

            // 4. Retornar 200 OK com o objeto atualizado
            return Ok(instituicao);
        }

        // ---
        //[cite_start]// MÉTODO PATCH (Ativar/Inativar) - (FA-1) [cite: 68]
        // Mapeado para: PATCH api/Instituicoes/{id}/estado
        // ---
        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> AtualizarEstadoInstituicao(Guid id, [FromBody] AtualizarEstadoRequest request)
        {
            var instituicao = await _context.Instituicoes.FindAsync(id);

            if (instituicao == null)
            {
                return NotFound(new { message = "Instituição não encontrada." });
            }

            //[cite_start]// 1. Implementar o Soft Delete (RN-01.4) 
            instituicao.IsAtiva = request.IsAtiva; //

            // 2. Salvar
            _context.Instituicoes.Update(instituicao);
            await _context.SaveChangesAsync();

            // 3. Retornar 200 OK
            return Ok(new { message = $"Estado da instituição '{instituicao.Nome}' atualizado para {instituicao.IsAtiva}." });
        }
    }
}
