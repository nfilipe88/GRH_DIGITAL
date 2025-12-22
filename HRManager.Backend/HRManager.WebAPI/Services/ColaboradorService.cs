using FluentValidation;
using HRManager.Application.Interfaces;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManager.WebAPI.Services
{
    public class ColaboradorService : IColaboradorService
    {
        private readonly HRManagerDbContext _context;
        private readonly ITenantService _tenantService;

        public ColaboradorService(HRManagerDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        public async Task<ColaboradorListDto?> GetByIdAsync(Guid id)
        {
            var colab = await _context.Colaboradores
                .Include(c => c.Cargo)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (colab == null) return null;

            return new ColaboradorListDto
            {
                Id = colab.Id,
                NomeCompleto = colab.NomeCompleto,
                Email = colab.EmailPessoal,
                Funcao = colab.Cargo?.Nome ?? "N/A",
                Departamento = colab.Departamento,
                //Status = colab.IsAtivo ? "Ativo" : "Inativo",
                IsAtivo = colab.IsAtivo
            };
        }

        public async Task<List<ColaboradorListDto>> GetAllAsync()
        {
            return await _context.Colaboradores
                .Include(c => c.Cargo)
                .Select(c => new ColaboradorListDto
                {
                    Id = c.Id,
                    NomeCompleto = c.NomeCompleto,
                    Email = c.EmailPessoal,
                    Funcao = c.Cargo != null ? c.Cargo.Nome : "N/A",
                    Departamento = c.Departamento,
                    //Status = c.IsAtivo ? "Ativo" : "Inativo",
                    IsAtivo = c.IsAtivo
                })
                .ToListAsync();
        }

        public async Task<ColaboradorDto> CreateAsync(CriarColaboradorRequest request)
        {
            // 1. Validar Perfil e Instituição
            Guid instituicaoAlvoId;

            if (_tenantService.IsMasterTenant)
            {
                // REGRA DE NEGÓCIO (Master): Deve selecionar a instituição
                if (!request.InstituicaoId.HasValue || request.InstituicaoId == Guid.Empty)
                {
                    throw new ArgumentException("O Gestor Master deve selecionar uma Instituição para o colaborador.");
                }
                instituicaoAlvoId = request.InstituicaoId.Value;
            }
            else
            {
                // REGRA DE NEGÓCIO (RH): Carrega automaticamente do Token
                var tenantId = _tenantService.TenantId;
                if (!tenantId.HasValue)
                {
                    throw new UnauthorizedAccessException("Não foi possível identificar a instituição do Gestor de RH.");
                }
                instituicaoAlvoId = tenantId.Value;
            }

            // 2. Verificar se a instituição existe (Segurança extra para o Master)
            var instituicaoExists = await _context.Instituicoes.AnyAsync(i => i.Id == instituicaoAlvoId);
            if (!instituicaoExists) throw new KeyNotFoundException("A instituição selecionada não existe.");

            // 3. Mapear e Criar (Forçando o ID correto)
            var colaborador = new Colaborador
            {
                NomeCompleto = request.NomeCompleto,
                EmailPessoal = request.EmailPessoal,
                DataNascimento = request.DataNascimento,
                NIF = request.NIF,
                Telemovel = request.Telemovel,
                DataAdmissao = request.DataAdmissao,
                Morada = request.Morada,
                InstituicaoId = instituicaoAlvoId,
                CargoId = request.CargoId,
                Departamento = request.Departamento,
                IBAN = request.IBAN,
                Localizacao = request.Localizacao,
                NumeroAgente = request.NumeroAgente,
                SalarioBase = request.SalarioBase,
                SaldoFerias = 22,
                TipoContrato = request.TipoContrato,
            };

            _context.Colaboradores.Add(colaborador);
            await _context.SaveChangesAsync();

            return MapToDto(colaborador);
        }

        public async Task<bool> UpdateAsync(Guid id, AtualizarDadosPessoaisRequest request)
        {
            var colab = await _context.Colaboradores.FindAsync(id);
            if (colab == null) return false;

            // Atualiza campos permitidos se não forem nulos ou vazios
            if (!string.IsNullOrEmpty(request.Telemovel.ToString()))
                colab.Telemovel = request.Telemovel;

            if (!string.IsNullOrEmpty(request.Morada))
                colab.Morada = request.Morada;

            if (!string.IsNullOrEmpty(request.IBAN))
                colab.IBAN = request.IBAN;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task ToggleAtivoAsync(Guid id)
        {
            var colaborador = await _context.Colaboradores.FindAsync(id);
            if (colaborador == null) throw new KeyNotFoundException("Colaborador não encontrado.");

            colaborador.IsAtivo = !colaborador.IsAtivo;
            await _context.SaveChangesAsync();
        }

        public async Task DesativarColaboradorAsync(Guid id)
        {
            var colaborador = await _context.Colaboradores
                .Include(c => c.Subordinados) // Importante carregar os subordinados
                .FirstOrDefaultAsync(c => c.Id == id);

            if (colaborador == null) throw new KeyNotFoundException("Colaborador não encontrado.");

            // Se tiver subordinados ativos, lançamos uma exceção específica
            if (colaborador.Subordinados.Any(s => s.IsAtivo))
            {
                // Usamos uma mensagem chave que o frontend vai procurar
                throw new InvalidOperationException("HAS_SUBORDINATES");
            }

            colaborador.IsAtivo = false;
            await _context.SaveChangesAsync();
        }

        public async Task TransferirEquipaAsync(Guid gestorAntigoId, Guid gestorNovoId)
        {
            // 1. Busca todos os subordinados do gestor antigo
            var subordinados = await _context.Colaboradores
                .Where(c => c.GestorId == gestorAntigoId)
                .ToListAsync();

            // 2. Atualiza para o novo gestor
            foreach (var sub in subordinados)
            {
                sub.GestorId = gestorNovoId;
            }

            await _context.SaveChangesAsync();
        }

        private ColaboradorDto MapToDto(Colaborador c)
        {
            return new ColaboradorDto
            {
                NomeCompleto = c.NomeCompleto,
                DataNascimento = c.DataNascimento,
                EmailPessoal = c.EmailPessoal,
                NIF = c.NIF,
                IBAN = c.IBAN,
                DataAdmissao = c.DataAdmissao,
                NumeroAgente = c.NumeroAgente,
                TipoContrato = c.TipoContrato,
                SalarioBase = c.SalarioBase,
                Morada = c.Morada,
                Localizacao = c.Localizacao,
                Departamento = c.Departamento,
                Telemovel = c.Telemovel,
                IsAtivo = c.IsAtivo,
                // Adiciona outros campos conforme necessário no teu DTO
                InstituicaoId = c.InstituicaoId,
                NomeInsituicao = c.Instituicao?.Nome,
                GestorId = c.GestorId,
                NomeGestor = c.Gestor?.NomeCompleto,
                CargoId = c.CargoId,
                NomeCargo = c.Cargo?.Nome ?? "Sem Cargo", // Supondo que tens a navegação
            };
        }
    }
}
