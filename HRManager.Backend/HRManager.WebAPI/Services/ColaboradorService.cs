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

        public async Task<Colaborador> CreateAsync(CriarColaboradorRequest request)
        {
            var tenantId = _tenantService.GetInstituicaoId();

            // Validar se o Cargo existe
            var cargo = await _context.Cargos
                .FirstOrDefaultAsync(c => c.Id == request.CargoId); // Agora usa ID direto

            if (cargo == null)
                throw new KeyNotFoundException("Cargo não encontrado.");

            var novo = new Colaborador
            {
                NomeCompleto = request.NomeCompleto,
                EmailPessoal = request.EmailPessoal,
                NIF = request.NIF,
                DataAdmissao = DateTime.SpecifyKind(request.DataAdmissao, DateTimeKind.Utc),
                CargoId = request.CargoId,
                Departamento = request.Departamento,
                SalarioBase = request.SalarioBase,
                InstituicaoId = tenantId,
                IsAtivo = true
            };

            _context.Colaboradores.Add(novo);
            await _context.SaveChangesAsync();
            return novo;
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

    }
}
