using HRManager.WebAPI.Constants;
using HRManager.WebAPI.Domain.enums;
using HRManager.WebAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HRManager.WebAPI.Data
{
    public static class DbSeeder
    {
        public static async Task Seed(HRManagerDbContext context, UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            // 1. Evita duplicar se a instituição já existir
            if (await context.Instituicoes.AnyAsync()) return;

            // --- IDs FIXOS (Para manter as relações consistentes) ---
            var empresaId = Guid.NewGuid();

            // IDs Users
            var adminUserId = Guid.NewGuid();
            var gestorUserId = Guid.NewGuid();
            var colabUserId = Guid.NewGuid();

            // IDs Colaboradores (Perfis)
            var perfilAdminId = Guid.NewGuid();
            var perfilGestorId = Guid.NewGuid();
            var perfilColabId = Guid.NewGuid();

            // ID Ciclo Avaliação
            var cicloId = Guid.NewGuid();

            // 2. Criar a Instituição
            var empresa = new Instituicao
            {
                Id = empresaId,
                Nome = "Tech Solutions Angola",
                NIF = "500000000",
                EmailContato = "geral@techsolutions.ao",
                Telemovel = 923000000,
                Endereco = "Talatona, Luanda",
                IsAtiva = true,
                DataCriacao = DateTime.UtcNow
            };
            context.Instituicoes.Add(empresa);
            await context.SaveChangesAsync();

            // 3. Criar Roles
            string[] roles = { RolesConstants.GestorMaster, RolesConstants.GestorRH, RolesConstants.Colaborador };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new Role { Name = roleName, Description = $"Perfil de {roleName}" });
                }
            }

            // 4. Criar Cargos
            var cargos = new List<Cargo>
            {
                new Cargo { Nome = "Diretor Geral", InstituicaoId = empresaId, IsAtivo = true },
                new Cargo { Nome = "Gestora de RH", InstituicaoId = empresaId, IsAtivo = true },
                new Cargo { Nome = "Full-Stack Developer", InstituicaoId = empresaId, IsAtivo = true }
            };
            context.Cargos.AddRange(cargos);
            await context.SaveChangesAsync();

            var cargoAdmin = await context.Cargos.FirstAsync(c => c.Nome == "Diretor Geral");
            var cargoGestor = await context.Cargos.FirstAsync(c => c.Nome == "Gestora de RH");
            var cargoDev = await context.Cargos.FirstAsync(c => c.Nome == "Full-Stack Developer");

            // 5. Criar Users (Password padrão: 123456 - user@123)
            // Nota: Calculamos a hash manualmente com BCrypt para ser compatível com o AuthService
            var passwordPadraoHash = BCrypt.Net.BCrypt.HashPassword("user@123");

            var adminUser = new User
            {
                Id = adminUserId,
                UserName = "admin@tech.com",
                Email = "admin@tech.com",
                NomeCompleto = "Sr. Administrador",
                InstituicaoId = empresaId,
                IsAtivo = true,
                EmailConfirmed = true,
                PasswordHash = passwordPadraoHash // <--- Hash definida manualmente
            };

            var gestorUser = new User
            {
                Id = gestorUserId,
                UserName = "rh@tech.com",
                Email = "rh@tech.com",
                NomeCompleto = "Maria Gestora",
                InstituicaoId = empresaId,
                IsAtivo = true,
                EmailConfirmed = true,
                PasswordHash = passwordPadraoHash
            };

            var colabUser = new User
            {
                Id = colabUserId,
                UserName = "joao@tech.com",
                Email = "joao@tech.com",
                NomeCompleto = "João Dev",
                InstituicaoId = empresaId,
                IsAtivo = true,
                EmailConfirmed = true,
                PasswordHash = passwordPadraoHash
            };

            // Criar utilizadores (sem passar a password como argumento, para manter a nossa hash BCrypt)
            if (await userManager.FindByIdAsync(adminUserId.ToString()) == null)
            {
                await userManager.CreateAsync(adminUser);
                await userManager.AddToRoleAsync(adminUser, RolesConstants.GestorMaster);
            }

            if (await userManager.FindByIdAsync(gestorUserId.ToString()) == null)
            {
                await userManager.CreateAsync(gestorUser);
                await userManager.AddToRoleAsync(gestorUser, RolesConstants.GestorRH);
            }

            if (await userManager.FindByIdAsync(colabUserId.ToString()) == null)
            {
                await userManager.CreateAsync(colabUser);
                await userManager.AddToRoleAsync(colabUser, RolesConstants.Colaborador);
            }

            // 6. Criar Perfis de Colaborador
            var colaboradores = new List<Colaborador>
            {
                new Colaborador { Id = perfilAdminId, UserId = adminUserId, InstituicaoId = empresaId, NomeCompleto = adminUser.NomeCompleto, EmailPessoal = adminUser.Email, CargoId = cargoAdmin.Id, Departamento = "Board", NIF = "000000001", DataAdmissao = DateTime.UtcNow.AddYears(-10), IsAtivo = true },
                new Colaborador { Id = perfilGestorId, UserId = gestorUserId, InstituicaoId = empresaId, NomeCompleto = gestorUser.NomeCompleto, EmailPessoal = gestorUser.Email, CargoId = cargoGestor.Id, Departamento = "RH", NIF = "000000002", DataAdmissao = DateTime.UtcNow.AddYears(-5), GestorId = perfilAdminId, IsAtivo = true },
                new Colaborador { Id = perfilColabId, UserId = colabUserId, InstituicaoId = empresaId, NomeCompleto = colabUser.NomeCompleto, EmailPessoal = colabUser.Email, CargoId = cargoDev.Id, Departamento = "IT", NIF = "000000003", DataAdmissao = DateTime.UtcNow.AddYears(-2), GestorId = perfilGestorId, IsAtivo = true }
            };
            context.Colaboradores.AddRange(colaboradores);

            // 7. Competências
            context.Competencias.AddRange(
                new Competencia { Nome = "C# Avançado", Tipo = TipoCompetencia.Tecnica, InstituicaoId = empresaId, IsAtiva = true },
                new Competencia { Nome = "Comunicação", Tipo = TipoCompetencia.Comportamental, InstituicaoId = empresaId, IsAtiva = true },
                new Competencia { Nome = "Trabalho em Equipa", Tipo = TipoCompetencia.Comportamental, InstituicaoId = empresaId, IsAtiva = true }
            );
            await context.SaveChangesAsync();

            // --- CENÁRIOS DE TESTE ---

            // 8. Ciclo de Avaliação Ativo
            var ciclo = new CicloAvaliacao
            {
                Id = cicloId,
                Nome = "Avaliação Anual 2024",
                DataInicio = DateTime.UtcNow.AddDays(-10),
                DataFim = DateTime.UtcNow.AddDays(20),
                IsAtivo = true,
                InstituicaoId = empresaId
            };
            context.CiclosAvaliacao.Add(ciclo);

            // 9. Avaliação iniciada para o João (Para ele fazer Autoavaliação)
            var avaliacao = new Avaliacao
            {
                Id = Guid.NewGuid(),
                CicloId = cicloId,
                ColaboradorId = perfilColabId, // João é o avaliado (Colaborador)
                GestorId = gestorUserId,       // Usar o ID do User, não o do Perfil/Colaborador
                Estado = EstadoAvaliacao.AnaliseGestor,
                DataCriacao = DateTime.UtcNow,
                InstituicaoId = empresaId
            };
            context.Avaliacoes.Add(avaliacao);

            // 10. Pedido de Ausência Pendente (João pede Férias)
            context.Ausencias.Add(new Ausencia
            {
                ColaboradorId = perfilColabId,
                Tipo = TipoAusencia.Ferias,
                DataInicio = DateTime.UtcNow.AddDays(5),
                DataFim = DateTime.UtcNow.AddDays(10),
                Estado = EstadoAusencia.Pendente,
                Motivo = "Férias anuais agendadas",
                InstituicaoId = empresaId
            });

            // 11. Pedido de Declaração (João pede Declaração de Rendimentos)
            context.PedidosDeclaracao.Add(new PedidoDeclaracao
            {
                ColaboradorId = perfilColabId,
                Tipo = TipoDeclaracao.ComprovativoVinculo,
                Observacoes = "Crédito Habitação",
                Estado = EstadoPedidoDeclaracao.Pendente,
                DataSolicitacao = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }
    }
}