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

        public InstituicaoService(HRManagerDbContext context)
        {
            _context = context;
        }

        public async Task<List<Instituicao>> GetAllAsync()
        {
            return await _context.Instituicoes
                .OrderBy(i => i.Nome)
                .ToListAsync();
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
                Telefone = request.Telefone,
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
            instituicao.Endereco = request.Endereco;
            instituicao.Telefone = request.Telefone;
            instituicao.EmailContato = request.EmailContato;

            // Nota: Geralmente não permitimos alterar o Slug ou NIF após a criação por questões de integridade

            await _context.SaveChangesAsync();
            return instituicao;
        }
    }
}
