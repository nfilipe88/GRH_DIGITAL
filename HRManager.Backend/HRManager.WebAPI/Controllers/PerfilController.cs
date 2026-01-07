using HRManager.Application.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HRManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PerfilController : ControllerBase
    {
        private readonly HRManagerDbContext _context;
        private readonly ITenantService _tenantService;
        private readonly IWebHostEnvironment _env;

        public PerfilController(HRManagerDbContext context, ITenantService tenantService, IWebHostEnvironment env)
        {
            _context = context;
            _tenantService = tenantService;
            _env = env;
        }

        // ---
        // GET: Obter Perfil Completo
        // Pode receber ?colaboradorId=X. Se não receber, devolve o do próprio utilizador logado.
        // ---
        [HttpGet]
        public async Task<IActionResult> GetPerfil([FromQuery] Guid? colaboradorId)
        {
            var colaborador = await GetColaboradorAlvo(colaboradorId);

            if (colaborador == null) return NotFound(new { message = "Colaborador não encontrado ou sem permissão." });

            var habilitacoes = await _context.HabilitacoesLiterarias
                .Where(h => h.ColaboradorId == colaborador.Id)
                .OrderByDescending(h => h.DataConclusao)
                .ToListAsync();

            var certificacoes = await _context.CertificacoesProfissionais
                .Where(c => c.ColaboradorId == colaborador.Id)
                .OrderByDescending(c => c.DataEmissao)
                .ToListAsync();

            var perfil = new PerfilDto
            {
                ColaboradorId = colaborador.Id,
                NomeCompleto = colaborador.NomeCompleto,
                // CORREÇÃO: Aceder ao nome do objeto Cargo e tratar nulo
                Cargo = colaborador.Cargo?.Nome ?? "Sem Cargo",
                Email = colaborador.EmailPessoal,
                Morada = colaborador.Morada,
                IBAN = colaborador.IBAN,

                Habilitacoes = habilitacoes.Select(h => new HabilitacaoDto
                {
                    Id = h.Id,
                    Grau = h.Grau.ToString(),
                    Curso = h.Curso,
                    Instituicao = h.InstituicaoEnsino,
                    DataConclusao = h.DataConclusao,
                    CaminhoDocumento = h.CaminhoDocumento
                }).ToList(),

                Certificacoes = certificacoes.Select(c => new CertificacaoDto
                {
                    Id = c.Id,
                    Nome = c.NomeCertificacao,
                    Entidade = c.EntidadeEmissora,
                    DataEmissao = c.DataEmissao,
                    DataValidade = c.DataValidade,
                    CaminhoDocumento = c.CaminhoDocumento
                }).ToList()
            };

            return Ok(perfil);
        }

        // ---
        // POST: Adicionar Habilitação
        // ---
        [HttpPost("habilitacoes")]
        public async Task<IActionResult> AddHabilitacao([FromQuery] Guid? colaboradorId, [FromForm] CriarHabilitacaoRequest request)
        {
            var colaborador = await GetColaboradorAlvo(colaboradorId);
            if (colaborador == null) return Unauthorized(new { message = "Sem permissão." });

            string? caminho = await UploadFicheiro(request.Documento, "habilitacoes");

            var habilitacao = new HabilitacaoLiteraria
            {
                ColaboradorId = colaborador.Id,
                Grau = request.Grau,
                Curso = request.Curso,
                InstituicaoEnsino = request.InstituicaoEnsino,
                DataConclusao = DateTime.SpecifyKind(request.DataConclusao, DateTimeKind.Utc),
                CaminhoDocumento = caminho
            };

            _context.HabilitacoesLiterarias.Add(habilitacao);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Habilitação adicionada com sucesso." });
        }

        // ---
        // POST: Adicionar Certificação
        // ---
        [HttpPost("certificacoes")]
        public async Task<IActionResult> AddCertificacao([FromQuery] Guid? colaboradorId, [FromForm] CriarCertificacaoRequest request)
        {
            var colaborador = await GetColaboradorAlvo(colaboradorId);
            if (colaborador == null) return Unauthorized(new { message = "Sem permissão." });

            string? caminho = await UploadFicheiro(request.Documento, "certificacoes");

            var certificacao = new CertificacaoProfissional
            {
                ColaboradorId = colaborador.Id,
                NomeCertificacao = request.NomeCertificacao,
                EntidadeEmissora = request.EntidadeEmissora,
                DataEmissao = DateTime.SpecifyKind(request.DataEmissao, DateTimeKind.Utc),
                DataValidade = request.DataValidade.HasValue ? DateTime.SpecifyKind(request.DataValidade.Value, DateTimeKind.Utc) : null,
                CaminhoDocumento = caminho
            };

            _context.CertificacoesProfissionais.Add(certificacao);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Certificação adicionada com sucesso." });
        }

        // ---
        // DELETE: Remover Habilitação
        // ---
        [HttpDelete("habilitacoes/{id}")]
        public async Task<IActionResult> DeleteHabilitacao(Guid id)
        {
            var item = await _context.HabilitacoesLiterarias.FindAsync(id);
            if (item == null) return NotFound();

            // Verificar se quem está a apagar tem permissão sobre o dono do item
            var colaborador = await GetColaboradorAlvo(item.ColaboradorId);
            if (colaborador == null) return Forbid();

            // (Opcional) Apagar ficheiro do disco aqui

            _context.HabilitacoesLiterarias.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Item removido." });
        }

        // ---
        // DELETE: Remover Certificação
        // ---
        [HttpDelete("certificacoes/{id}")]
        public async Task<IActionResult> DeleteCertificacao(int id)
        {
            var item = await _context.CertificacoesProfissionais.FindAsync(id);
            if (item == null) return NotFound();

            var colaborador = await GetColaboradorAlvo(item.ColaboradorId);
            if (colaborador == null) return Forbid();

            _context.CertificacoesProfissionais.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Item removido." });
        }


        // ==========================================
        // MÉTODOS AUXILIARES (Lógica de Segurança e Upload)
        // ==========================================

        /// <summary>
        /// Determina qual o colaborador alvo da ação, aplicando regras de segurança.
        /// </summary>
        private async Task<Colaborador?> GetColaboradorAlvo(Guid? idSolicitado)
        {
            // 1. Se um ID foi pedido (Gestão)
            if (idSolicitado.HasValue)
            {
                // Validar se é Gestor
                if (!User.IsInRole("GestorMaster") && !User.IsInRole("GestorRH"))
                {
                    return null; // Colaborador normal não pode pedir dados de outros (idSolicitado)
                }

                var colaborador = await _context.Colaboradores.FindAsync(idSolicitado.Value);
                if (colaborador == null) return null;

                // Se for GestorRH, validar Instituição (Multi-tenant)
                if (User.IsInRole("GestorRH"))
                {
                    var tenantId = _tenantService.GetTenantId();
                    if (colaborador.InstituicaoId != tenantId) return null; // Tentou aceder a outra empresa
                }

                return colaborador;
            }

            // 2. Se nenhum ID foi pedido (Auto-serviço / "O Meu Perfil")
            else
            {
                var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
                return await _context.Colaboradores.FirstOrDefaultAsync(c => c.EmailPessoal == email);
            }
        }

        private async Task<string?> UploadFicheiro(IFormFile? ficheiro, string subPasta)
        {
            if (ficheiro == null || ficheiro.Length == 0) return null;

            var pastaDestino = Path.Combine(_env.WebRootPath, "uploads", subPasta);
            if (!Directory.Exists(pastaDestino)) Directory.CreateDirectory(pastaDestino);

            var nomeUnico = Guid.NewGuid().ToString() + Path.GetExtension(ficheiro.FileName);
            var caminhoCompleto = Path.Combine(pastaDestino, nomeUnico);

            using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
            {
                await ficheiro.CopyToAsync(stream);
            }

            return $"uploads/{subPasta}/{nomeUnico}";
        }

        // ---
        // PUT: Atualizar Dados Pessoais (Morada, IBAN)
        // ---
        [HttpPut("dados-pessoais")]
        public async Task<IActionResult> AtualizarDadosPessoais([FromQuery] Guid? colaboradorId, [FromBody] AtualizarDadosPessoaisRequest request)
        {
            var colaborador = await GetColaboradorAlvo(colaboradorId);
            if (colaborador == null) return Unauthorized(new { message = "Sem permissão." });

            // Atualizar apenas os campos permitidos
            colaborador.Morada = request.Morada;
            colaborador.IBAN = request.IBAN;

            _context.Colaboradores.Update(colaborador);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Dados pessoais atualizados com sucesso." });
        }
    }
}
