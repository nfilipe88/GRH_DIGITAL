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
        private readonly IEmailService _emailService;
        // Podes injetar IValidator<CriarColaboradorRequest> se quiseres validação manual aqui dentro

        public ColaboradorService(HRManagerDbContext context, ITenantService tenantService, IEmailService emailService)
        {
            _context = context;
            _tenantService = tenantService;
            _emailService = emailService;
        }

        public async Task<List<ColaboradorListDto>> GetAllAsync(Guid? instituicaoId = null)
        {
            var query = _context.Colaboradores.AsQueryable();

            // Se for Gestor Master e passar ID, filtra. 
            // Se for GestorRH, o Global Filter do DbContext JÁ filtra automaticamente pelo TenantService.
            if (instituicaoId.HasValue)
            {
                query = query.Where(c => c.InstituicaoId == instituicaoId.Value);
            }

            return await query
                .Include(c => c.Instituicao)
                .Select(c => new ColaboradorListDto
                {
                    Id = c.Id,
                    NomeCompleto = c.NomeCompleto,
                    Cargo = c.Cargo,
                    Departamento = c.Departamento,
                    EmailPessoal = c.EmailPessoal,
                    NomeInstituicao = c.Instituicao.Nome,
                    IsAtivo = c.IsAtivo
                })
                .ToListAsync();
        }

        public async Task<UserDetailsDto?> GetByIdAsync(int id)
        {
            var c = await _context.Colaboradores
                                  .Include(x => x.Instituicao)
                                  .FirstOrDefaultAsync(x => x.Id == id);

            if (c == null) return null;

            return new UserDetailsDto
            {
                Id = c.Id,
                NomeCompleto = c.NomeCompleto,
                Email = c.EmailPessoal,
                Cargo = c.Cargo
            };
        }

        public async Task<UserDetailsDto> CreateAsync(CriarColaboradorRequest request)
        {
            // 1. Validação de Negócio: NIF Único (O validador fluente vê o formato, aqui vemos a duplicidade)
            // Nota: O DbContext já tem UniqueIndex, mas é bom validar antes de estourar erro de SQL.
            bool existe = await _context.Colaboradores.AnyAsync(c => c.NIF == request.NIF);
            if (existe) throw new ValidationException("Já existe um colaborador com este NIF nesta instituição.");

            // 2. Mapeamento
            var novoColaborador = new Colaborador
            {
                NomeCompleto = request.NomeCompleto,
                EmailPessoal = request.EmailPessoal,
                NIF = request.NIF,
                // Force UTC for DataAdmissao
                DataAdmissao = DateTime.SpecifyKind(request.DataAdmissao, DateTimeKind.Utc),
                // Force UTC for DataNascimento (if present)
                DataNascimento = request.DataNascimento.HasValue? DateTime.SpecifyKind(request.DataNascimento.Value, DateTimeKind.Utc) : null,
                // -----------------------
                Cargo = request.Cargo,
                TipoContrato = request.TipoContrato,
                NumeroAgente = request.NumeroAgente,
                Departamento = request.Departamento,
                SalarioBase = request.SalarioBase,
                Telemovel = request.Telemovel,
                Morada = request.Morada,
                IBAN = request.IBAN,
                Localizacao = request.Localizacao,
                IsAtivo = true,
                SaldoFerias = 22, // Valor default, ou vir de config, por implementar depois
                //InstituicaoId = (Guid)request.InstituicaoId
            };

            _context.Colaboradores.Add(novoColaborador);
            await _context.SaveChangesAsync();

            // 3. (Opcional) Enviar Email de Boas Vindas
            try
            {
                await _emailService.SendEmailAsync(novoColaborador.EmailPessoal, "Bem-vindo! " + novoColaborador.NomeCompleto, "A sua conta de colaborador foi criada.");
            }
            catch{ }

            return await GetByIdAsync(novoColaborador.Id);
        }

        public async Task<UserDetailsDto> UpdateAsync(int id, AtualizarDadosPessoaisRequest request)
        {
            var colaborador = await _context.Colaboradores.FindAsync(id);
            if (colaborador == null) throw new KeyNotFoundException("Colaborador não encontrado.");

            // Atualizar campos permitidos
            colaborador.Telemovel = request.Telemovel;
            colaborador.Morada = request.Morada;
            colaborador.IBAN = request.IBAN;

            // Se for permitido alterar cargo/salário aqui, adicionar mapeamento

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task ToggleAtivoAsync(int id)
        {
            var colaborador = await _context.Colaboradores.FindAsync(id);
            if (colaborador == null) throw new KeyNotFoundException("Colaborador não encontrado.");

            colaborador.IsAtivo = !colaborador.IsAtivo;
            await _context.SaveChangesAsync();
        }

    }
}
