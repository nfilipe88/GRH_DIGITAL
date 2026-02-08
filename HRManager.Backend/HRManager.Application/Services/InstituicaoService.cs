using FluentValidation;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManager.WebAPI.Services
{
    public class InstituicaoService : IInstituicaoService
    {
        private readonly HRManagerDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InstituicaoService(HRManagerDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<InstituicaoListDto>> GetAllAsync()
        {
            try
            {
                // 1. Inicia a query SEM executar
                var query = _context.Instituicoes
                    .Include(i => i.Colaboradores)
                    .AsQueryable();

                // 2. Verifica se é GestorMaster
                var user = _httpContextAccessor.HttpContext?.User;
                bool isMaster = user?.IsInRole("GestorMaster") ?? false;

                // 3. Se NÃO for master, aplica filtro
                if (!isMaster)
                {
                    var tenantIdClaim = user?.FindFirst("tenantId")?.Value;
                    if (Guid.TryParse(tenantIdClaim, out var tenantId))
                    {
                        query = query.Where(i => i.Id == tenantId);
                    }
                    else
                    {
                        // Se não tem tenantId e não é master, retorna lista vazia
                        return new List<InstituicaoListDto>();
                    }
                }
                // Se FOR master, mostra todas as instituições

                // 4. Executa a query e converte para DTO
                var instituicoes = await query.ToListAsync();
                return instituicoes.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter instituições: {ex.Message}");
                throw;
            }
        }
        public async Task<Instituicao?> GetByIdAsync(Guid id)
        {
            return await _context.Instituicoes.FindAsync(id);
        }

        public async Task<Instituicao?> GetBySlugAsync(string slug)
        {
            return await _context.Instituicoes
                .FirstOrDefaultAsync(i => i.IdentificadorUnico == slug);
        }

        public async Task<Instituicao> CreateAsync(CriarInstituicaoRequest request)
        {
            // RN-01.1: Validar Unicidade do Slug
            bool slugExiste = await _context.Instituicoes
                .AnyAsync(i => i.IdentificadorUnico == request.IdentificadorUnico);

            if (slugExiste)
                throw new ValidationException($"O identificador '{request.IdentificadorUnico}' já está em uso.");

            // Validar NIF duplicado (opcional, mas recomendado)
            bool NIFExiste = await _context.Instituicoes.AnyAsync(i => i.NIF == request.NIF);
            if (NIFExiste)
                throw new ValidationException($"Já existe uma instituição com o NIF '{request.NIF}'.");

            var novaInstituicao = new Instituicao
            {
                Id = Guid.NewGuid(),
                Nome = request.Nome,
                IdentificadorUnico = request.IdentificadorUnico.ToUpper(),
                NIF = request.NIF,
                Endereco = request.Endereco,
                Telemovel = request.Telemovel,
                EmailContato = request.EmailContato,
                DataCriacao = DateTime.UtcNow,
                IsAtiva = true
            };

            _context.Instituicoes.Add(novaInstituicao);
            await _context.SaveChangesAsync();

            return novaInstituicao;
        }

        public async Task<Instituicao> UpdateAsync(Guid id, AtualizarInstituicaoRequest request)
        {
            var instituicao = await _context.Instituicoes.FindAsync(id);
            if (instituicao == null)
                throw new KeyNotFoundException("Instituição não encontrada.");

            instituicao.Nome = request.Nome;
            instituicao.IdentificadorUnico = request.IdentificadorUnico;
            instituicao.NIF = request.NIF;
            instituicao.Endereco = request.Endereco;
            instituicao.Telemovel = request.Telemovel;
            instituicao.EmailContato = request.EmailContato;

            // Nota: Geralmente não permitimos alterar o Slug ou NIF após a criação por questões de integridade

            await _context.SaveChangesAsync();
            return instituicao;
        }

        // Adiciona este método auxiliar para obter o tenantId
        private Guid? GetTenantId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var tenantIdClaim = user?.FindFirst("tenantId")?.Value;

            if (Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                return tenantId;
            }

            return null;
        }

        private InstituicaoListDto MapToDto(Instituicao instituicao)
        {
            return new InstituicaoListDto
            {
                Id = instituicao.Id,
                Nome = instituicao.Nome,
                IdentificadorUnico = instituicao.IdentificadorUnico,
                NIF = instituicao.NIF,
                Endereco = instituicao.Endereco,
                Telemovel = instituicao.Telemovel,
                EmailContato = instituicao.EmailContato,
                IsAtiva = instituicao.IsAtiva,
                //ColaboradoresCount = instituicao.Colaboradores?.Count ?? 0
            };
        }
    }
}
