using FluentValidation;
using HRManager.Application.Interfaces;
using HRManager.WebAPI.Constants;
using HRManager.WebAPI.Domain.enums;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Helpers;
using HRManager.WebAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HRManager.WebAPI.Services
{
    public class ColaboradorService : IColaboradorService
    {
        private readonly HRManagerDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ITenantService _tenantService;

        public ColaboradorService(HRManagerDbContext context, ITenantService tenantService, UserManager<User> userManager)
        {
            _context = context;
            _tenantService = tenantService;
            _userManager = userManager;
        }

        
        public async Task<ColaboradorDto?> GetByIdAsync(Guid id)
        {
            var colab = await _context.Colaboradores
                .Include(c => c.Cargo)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (colab == null) return null;

            return new ColaboradorDto
            {
                Id = colab.Id,
                NomeCompleto = colab.NomeCompleto,
                EmailPessoal = colab.EmailPessoal,
                NIF = colab.NIF,
                NumeroAgente = colab.NumeroAgente,
                Morada = colab.Morada,
                Telemovel = colab.Telemovel,
                SalarioBase = colab.SalarioBase,
                TipoContrato = colab.TipoContrato,
                SaldoFerias = colab.SaldoFerias,
                Departamento = colab.Departamento,
                Localizacao = colab.Localizacao,
                DataAdmissao = colab.DataAdmissao,
                DataNascimento = colab.DataNascimento,
                IBAN = colab.IBAN,
                CargoId = colab.CargoId,
                NomeCargo = colab.Cargo != null ? colab.Cargo.Nome : "N/A",
                InstituicaoId = colab.InstituicaoId,
                NomeInsituicao = colab.Instituicao != null ? colab.Instituicao.Nome : "N/A",
                GestorId = colab.GestorId,
                NomeGestor = colab.Gestor != null ? colab.Gestor.NomeCompleto : "N/A",
            };
        }

        /// <summary>
        /// Metodo para obter a lista de todos os colaboradores registados na plataforma.
        /// </summary>
        /// <returns></returns>
        public async Task<List<ColaboradorListDto>> GetAllAsync()
        {
            return await _context.Colaboradores
                .Include(c => c.Cargo)
                .Select(c => new ColaboradorListDto
                {
                    Id = c.Id,
                    NomeCompleto = c.NomeCompleto,
                    Email = c.EmailPessoal,
                    Cargo = c.Cargo != null ? c.Cargo.Nome : "N/A",
                    NomeInstituicao=c.Instituicao != null ? c.Instituicao.Nome : "N/A",
                    IsAtivo = c.IsAtivo
                })
                .ToListAsync();
        }

        /// <summary>
        /// Metodo para obter a lista de colaboradores por instituição.
        /// Para uso do RH ver apenas os colaboradores da sua empresa.
        /// </summary>
        /// <param name="instituicaoID"></param>
        /// <returns></returns>
        public async Task<List<ColaboradorListDto>> GetAllByInstituicaoAsync(Guid instituicaoID)
        {
            return await _context.Colaboradores
                .Where(c => c.InstituicaoId == instituicaoID)
                .Include(c => c.Cargo)
                .Select(c => new ColaboradorListDto
                {
                    Id = c.Id,
                    NomeCompleto = c.NomeCompleto,
                    Email = c.EmailPessoal,
                    Cargo = c.Cargo != null ? c.Cargo.Nome : "N/A",
                    IsAtivo = c.IsAtivo
                })
                .ToListAsync();
        }

        public async Task<ColaboradorDto> CreateAsync(CriarColaboradorRequest request)
        {
            // 1. Validações Iniciais
            if (await _context.Colaboradores.AnyAsync(c => c.EmailPessoal == request.EmailPessoal))
                throw new ValidationException("Já existe um colaborador com este email.");

            if (await _userManager.FindByEmailAsync(request.EmailPessoal) != null)
                throw new ValidationException("Este email já está registado como utilizador no sistema.");

            // 2. Iniciar Transação (Garante integridade: ou cria os dois ou nenhum)
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Gerar Password Aleatória Segura
                string randomPassword = PasswordGenerator.Generate(12);

                // 3. Criar o Utilizador (Identity)
                var newUser = new User
                {
                    UserName = request.EmailPessoal,
                    Email = request.EmailPessoal,
                    NomeCompleto = request.NomeCompleto, // Sincroniza o nome
                    InstituicaoId = _tenantService.GetTenantId() ?? Guid.Empty,
                    IsAtivo = true,
                    MustChangePassword = true, // Força a mudança de password no primeiro login
                };

                // Define uma password inicial (pode ser enviada por email depois)
                var result = await _userManager.CreateAsync(newUser, randomPassword);

                if (!result.Succeeded)
                    throw new ValidationException($"Erro ao criar utilizador: {string.Join(", ", result.Errors.Select(e => e.Description))}");

                // Atribui a Role de "Colaborador" por defeito
                await _userManager.AddToRoleAsync(newUser, RolesConstants.Colaborador);

                // 4. Criar o Colaborador (Domínio) vinculado ao Utilizador
                var novoColaborador = new Colaborador
                {
                    NomeCompleto = request.NomeCompleto,
                    EmailPessoal = request.EmailPessoal,
                    NIF = request.NIF,
                    DataNascimento = request.DataNascimento,
                    Morada = request.Morada,
                    Telemovel = request.Telemovel,
                    DataAdmissao = request.DataAdmissao,
                    TipoContrato = request.TipoContrato,
                    SalarioBase = request.SalarioBase,
                    IBAN = request.IBAN,
                    Departamento = request.Departamento,
                    Localizacao = request.Localizacao,
                    IsAtivo = true,
                    SaldoFerias = 22,
                    // O VÍNCULO ACONTECE AQUI:
                    GestorId = request.GestorId,
                    CargoId = request.CargoId,
                    InstituicaoId = _tenantService.GetTenantId() ?? Guid.Empty,
                    UserId = newUser.Id
                };

                _context.Colaboradores.Add(novoColaborador);
                await _context.SaveChangesAsync();

                // 4. Enviar Email com as Credenciais
                // Nota: Assumindo que o seu EmailService tem este método.
                // Se não tiver, precisaremos criar ou usar um placeholder.
                try
                {
                    // Injetar IEmailService no construtor se ainda não estiver
                    // private readonly IEmailService _emailService; 

                    /* IMPLEMENTAÇÃO DO EMAIL */
                    var assunto = "Bem-vindo ao HR Manager - Credenciais de Acesso";
                    var corpo = $@"
                        <h3>Olá {request.NomeCompleto},</h3>
                        <p>A sua conta foi criada com sucesso.</p>
                        <p>As suas credenciais provisórias são:</p>
                        <ul>
                            <li><b>Email:</b> {request.EmailPessoal}</li>
                            <li><b>Password:</b> {randomPassword}</li>
                        </ul>
                        <p>Por favor, altere a sua password no primeiro acesso.</p>";

                    // Descomente a linha abaixo quando o EmailService estiver pronto
                    // await _emailService.SendEmailAsync(request.EmailPessoal, assunto, corpo);

                    // Para testes locais, imprime na consola:
                    Console.WriteLine($"📧 EMAIL SIMULADO PARA: {request.EmailPessoal} | ASSUNTO: {assunto} | PASS: {randomPassword}");
                }
                catch (Exception ex)
                {
                    // Não queremos cancelar a criação se o email falhar, mas devemos logar
                    Console.WriteLine($"Erro ao enviar email: {ex.Message}");
                }

                // 5. Commit da Transação
                await transaction.CommitAsync();

                return MapToDto(novoColaborador);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw; // Relança o erro para o controlador tratar
            }
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

        /// <summary>
        /// Metodo para obter a lista de cargos ativos.
        /// </summary>
        /// <returns></returns>
        public async Task<List<CargoDto>> GetCargosAsync()
        {
            // O filtro global do DbContext (Tenant) aplica-se automaticamente aqui
            return await _context.Cargos
                .Where(c => c.IsAtivo)
                .Select(c => new CargoDto
                {
                    Id = c.Id,
                    Nome = c.Nome
                })
                .OrderBy(c => c.Nome)
                .ToListAsync();
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
