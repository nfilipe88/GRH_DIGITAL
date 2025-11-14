using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ColaboradoresController : ControllerBase
    {
        private readonly HRManagerDbContext _context;

        public ColaboradoresController(HRManagerDbContext context)
        {
            _context = context;
        }

        // ---
        // NOVO MÉTODO GET (Listar)
        // Mapeado para: GET api/Colaboradores
        // ---
        //[HttpGet]
        //public async Task<IActionResult> GetColaboradores()
        //{
        //    var colaboradores = await _context.Colaboradores
        //        .Include(c => c.Instituicao) // <-- A MÁGICA ACONTECE AQUI
        //        .ToListAsync();

        //    return Ok(colaboradores);
        //}

        // ---
        // MÉTODO GET (Listar) - VERSÃO OTIMIZADA COM DTO
        // Mapeado para: GET api/Colaboradores
        // ---
        [HttpGet]
        public async Task<IActionResult> GetColaboradores()
        {
            var colaboradores = await _context.Colaboradores
                .Select(c => new ColaboradorListDto
                {
                    Id = c.Id,
                    NomeCompleto = c.NomeCompleto,
                    EmailPessoal = c.EmailPessoal,
                    NIF = c.NIF,
                    Cargo = c.Cargo,
                    NomeInstituicao = c.Instituicao.Nome,

                    // *** ADICIONE ESTA LINHA ***
                    IsAtivo = c.IsAtivo
                })
                .ToListAsync();

            return Ok(colaboradores);
        }

        // ---
        // MÉTODO POST (Cadastrar)
        // Mapeado para: POST api/Colaboradores
        //[cite_start]// Baseado no CU-02 [cite: 75]
        // ---
        [HttpPost]
        public async Task<IActionResult> CriarColaborador([FromBody] CriarColaboradorRequest request)
        {
            // 1. Validar se a Instituição selecionada existe
            var instituicao = await _context.Instituicoes.FindAsync(request.InstituicaoId);
            if (instituicao == null)
            {
                return BadRequest(new { message = "Instituição selecionada é inválida." });
            }

            //[cite_start]// 2. Validar Regra de Negócio (RN-02.1): Unicidade do NIF por Instituição [cite: 88]
            if (await _context.Colaboradores.AnyAsync(c => c.NIF == request.NIF && c.InstituicaoId == request.InstituicaoId))
            {
                return BadRequest(new { message = $"O NIF '{request.NIF}' já está registado nesta instituição." });
            }

            // 3. Mapear o DTO (pedido) para o Modelo (base de dados)
            var novoColaborador = new Colaborador
            {
                // Dados Pessoais
                NomeCompleto = request.NomeCompleto,
                NIF = request.NIF,
                NumeroAgente = request.NumeroAgente,
                EmailPessoal = request.EmailPessoal,

                // CORREÇÃO AQUI:
                DataNascimento = request.DataNascimento.HasValue
                    ? DateTime.SpecifyKind(request.DataNascimento.Value, DateTimeKind.Utc)
                    : null,

                // Dados Contratuais
                // E CORREÇÃO AQUI:
                DataAdmissao = DateTime.SpecifyKind(request.DataAdmissao, DateTimeKind.Utc),

                Cargo = request.Cargo,
                TipoContrato = request.TipoContrato,
                SalarioBase = request.SalarioBase,

                // Organização
                Departamento = request.Departamento,
                Localizacao = request.Localizacao,

                // Relação
                InstituicaoId = request.InstituicaoId
            };

            // 4. Adicionar ao DbContext e Salvar
            _context.Colaboradores.Add(novoColaborador);
            await _context.SaveChangesAsync();

            //[cite_start]// 5. Retornar uma resposta 201 Created (Confirmação [cite: 117])
            return StatusCode(201, new { message = $"Colaborador '{novoColaborador.NomeCompleto}' criado com sucesso." });
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
            var colaborador = await _context.Colaboradores
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (colaborador == null)
            {
                return NotFound(new { message = "Colaborador não encontrado." });
            }

            // Nota: Retornamos o MODELO completo, pois o formulário de edição
            // precisa de todos os campos.
            return Ok(colaborador);
        }

        // ---
        // NOVO MÉTODO PUT (Atualizar)
        // Mapeado para: PUT api/Colaboradores/{id}
        // ---
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarColaborador(int id, [FromBody] CriarColaboradorRequest request)
        {
            // Reutilizamos o DTO 'CriarColaboradorRequest' para a atualização

            var colaborador = await _context.Colaboradores.FirstOrDefaultAsync(c => c.Id == id);

            if (colaborador == null)
            {
                return NotFound(new { message = "Colaborador não encontrado." });
            }

            // Validar Regra de Negócio (RN-02.1): Unicidade do NIF
            if (await _context.Colaboradores.AnyAsync(c => c.NIF == request.NIF && c.InstituicaoId == request.InstituicaoId && c.Id != id))
            {
                return BadRequest(new { message = $"O NIF '{request.NIF}' já está registado nesta instituição." });
            }

            // --- Mapear os dados do DTO para o modelo da BD ---
            // Dados Pessoais
            colaborador.NomeCompleto = request.NomeCompleto;
            colaborador.NIF = request.NIF;
            colaborador.NumeroAgente = request.NumeroAgente;
            colaborador.EmailPessoal = request.EmailPessoal;
            // Corrigir o DateTimeKind
            colaborador.DataNascimento = request.DataNascimento.HasValue
                ? DateTime.SpecifyKind(request.DataNascimento.Value, DateTimeKind.Utc)
                : null;

            // Dados Contratuais
            colaborador.DataAdmissao = DateTime.SpecifyKind(request.DataAdmissao, DateTimeKind.Utc);
            colaborador.Cargo = request.Cargo;
            colaborador.TipoContrato = request.TipoContrato;
            colaborador.SalarioBase = request.SalarioBase;

            // Organização
            colaborador.Departamento = request.Departamento;
            colaborador.Localizacao = request.Localizacao;
            colaborador.InstituicaoId = request.InstituicaoId;
            // --- Fim do Mapeamento ---

            _context.Colaboradores.Update(colaborador);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Colaborador '{colaborador.NomeCompleto}' atualizado com sucesso." });
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

            var colaborador = await _context.Colaboradores.FindAsync(id);

            if (colaborador == null)
            {
                return NotFound(new { message = "Colaborador não encontrado." });
            }

            colaborador.IsAtivo = request.IsAtiva;
            // No futuro, aqui adicionaremos o "MotivoInatividade"

            _context.Colaboradores.Update(colaborador);
            await _context.SaveChangesAsync();

            string acao = colaborador.IsAtivo ? "reativado" : "desativado";
            return Ok(new { message = $"Colaborador '{colaborador.NomeCompleto}' {acao} com sucesso." });
        }

        // ---
        // NOVO MÉTODO DELETE (Eliminar)
        // Mapeado para: DELETE api/Colaboradores/{id}
        // ---
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarColaborador(int id)
        {
            var colaborador = await _context.Colaboradores.FindAsync(id);

            if (colaborador == null)
            {
                return NotFound(new { message = "Colaborador não encontrado." });
            }

            // Implementa um "Hard Delete" (eliminação física)
            _context.Colaboradores.Remove(colaborador);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Colaborador '{colaborador.NomeCompleto}' eliminado com sucesso." });
        }
    }
}
