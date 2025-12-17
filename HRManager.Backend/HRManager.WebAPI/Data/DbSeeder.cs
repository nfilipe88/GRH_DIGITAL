using HRManager.WebAPI.Constants;
using HRManager.WebAPI.Domain.enums;
using HRManager.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManager.WebAPI.Data
    {
        public static class DbSeeder
        {
            public static void Seed(HRManagerDbContext context)
            {
                // 1. Garante que a BD existe
                context.Database.EnsureCreated();

                // 2. Se já existe uma Instituição, assume que o seed já correu e para.
                if (context.Instituicoes.Any()) return;

                // --- IDs FIXOS (Para garantir integridade nas relações) ---
                var empresaId = Guid.NewGuid();

                // IDs dos Cargos
                var cargoAdminId = Guid.NewGuid();
                var cargoDiretorId = Guid.NewGuid();
                var cargoDevId = Guid.NewGuid();

                // IDs dos Perfis (Colaboradores)
                var perfilMasterId = Guid.NewGuid();
                var perfilGestorId = Guid.NewGuid();
                var perfilColabId = Guid.NewGuid();

                // 3. Criar a Instituição
                var empresa = new Instituicao
                {
                    Id = empresaId,
                    Nome = "Tech Solutions Angola",
                    NIF = "500000000",
                    IdentificadorUnico = "tech-solutions",
                    EmailContato = "contacto@techsolutions.ao",
                    Telemovel = 923000000,
                    Endereco = "Talatona, Luanda, Angola",
                    IsAtiva = true,
                    DataCriacao = DateTime.UtcNow
                };
                context.Instituicoes.Add(empresa);

                // 4. Criar ou Obter Roles
                var roleMasterId = Guid.NewGuid();
                var roleGestorId = Guid.NewGuid();
                var roleColabId = Guid.NewGuid();

                if (!context.Roles.Any())
                {
                    var roles = new List<Role>
                {
                    new Role { Id = roleMasterId, Name = RolesConstants.GestorMaster, Description = "Acesso total ao sistema", InstituicaoId = Guid.Empty },
                    new Role { Id = roleGestorId, Name = RolesConstants.GestorRH, Description = "Gestão de RH e Equipas", InstituicaoId = Guid.Empty },
                    new Role { Id = roleColabId, Name = RolesConstants.Colaborador, Description = "Acesso pessoal (Self-Service)", InstituicaoId = Guid.Empty }
                };
                    context.Roles.AddRange(roles);
                }
                else
                {
                    // Se as roles já existirem (ex: criadas manualmente), buscamos os IDs
                    roleMasterId = context.Roles.First(r => r.Name == RolesConstants.GestorMaster).Id;
                    roleGestorId = context.Roles.First(r => r.Name == RolesConstants.GestorRH).Id;
                    roleColabId = context.Roles.First(r => r.Name == RolesConstants.Colaborador).Id;
                }

                context.SaveChanges();

                // 5. Criar Cargos
                var cargos = new List<Cargo>
            {
                new Cargo { Id = cargoAdminId, Nome = "Diretor Geral", InstituicaoId = empresaId, IsAtivo = true },
                new Cargo { Id = cargoDiretorId, Nome = "Gestora de RH", InstituicaoId = empresaId, IsAtivo = true },
                new Cargo { Id = cargoDevId, Nome = "Desenvolvedor Sénior", InstituicaoId = empresaId, IsAtivo = true }
            };
                context.Cargos.AddRange(cargos);
                context.SaveChanges();

                // 6. Criar Users (Login)
                // Nota: A senha para todos é "123456"
                var passwordHash = BCrypt.Net.BCrypt.HashPassword("123456");

                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    NomeCompleto = "Administrador Sistema",
                    Email = "admin@tech.com",
                    PasswordHash = passwordHash,
                    InstituicaoId = empresaId,
                    IsAtivo = true,
                    DataCriacao = DateTime.UtcNow
                };

                var gestorUser = new User
                {
                    Id = Guid.NewGuid(),
                    NomeCompleto = "Maria Gestora",
                    Email = "rh@tech.com",
                    PasswordHash = passwordHash,
                    InstituicaoId = empresaId,
                    IsAtivo = true,
                    DataCriacao = DateTime.UtcNow
                };

                var colabUser = new User
                {
                    Id = Guid.NewGuid(),
                    NomeCompleto = "João Desenvolvedor",
                    Email = "joao@tech.com",
                    PasswordHash = passwordHash,
                    InstituicaoId = empresaId,
                    IsAtivo = true,
                    DataCriacao = DateTime.UtcNow
                };

                context.Users.AddRange(adminUser, gestorUser, colabUser);
                context.SaveChanges();

                // 7. Associar Users às Roles
                context.UserRoles.AddRange(
                    new UserRole { UserId = adminUser.Id, RoleId = roleMasterId },
                    new UserRole { UserId = gestorUser.Id, RoleId = roleGestorId },
                    new UserRole { UserId = colabUser.Id, RoleId = roleColabId }
                );
                context.SaveChanges();

                // 8. Criar Perfis de Colaborador (Com Hierarquia)

                // 8.1 - Perfil ADMIN (Topo da pirâmide)
                var perfilAdmin = new Colaborador
                {
                    Id = perfilMasterId,
                    UserId = adminUser.Id,
                    InstituicaoId = empresaId,
                    NomeCompleto = adminUser.NomeCompleto,
                    EmailPessoal = adminUser.Email,
                    CargoId = cargoAdminId, // Diretor Geral
                    Departamento = "Administração",
                    NIF = "000000001",
                    DataNascimento = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    DataAdmissao = DateTime.UtcNow.AddYears(-10),
                    Morada = "Mutamba, Luanda",
                    Telemovel = 900000001,
                    SaldoFerias = 30,
                    TipoContrato = "Indeterminado",
                    IsAtivo = true,
                    GestorId = null // Ninguém manda no Admin
                };

                // 8.2 - Perfil GESTOR RH (Reporta ao Admin)
                var perfilGestor = new Colaborador
                {
                    Id = perfilGestorId,
                    UserId = gestorUser.Id,
                    InstituicaoId = empresaId,
                    NomeCompleto = gestorUser.NomeCompleto,
                    EmailPessoal = gestorUser.Email,
                    CargoId = cargoDiretorId, // Gestora RH
                    Departamento = "Recursos Humanos",
                    NIF = "000000002",
                    DataNascimento = new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                    DataAdmissao = DateTime.UtcNow.AddYears(-5),
                    Morada = "Kilamba, Luanda",
                    Telemovel = 900000002,
                    SaldoFerias = 22,
                    TipoContrato = "Indeterminado",
                    IsAtivo = true,
                    GestorId = perfilMasterId // <-- Reporta ao Admin
                };

                // 8.3 - Perfil COLABORADOR (Reporta à Gestora de RH)
                var perfilColab = new Colaborador
                {
                    Id = perfilColabId,
                    UserId = colabUser.Id,
                    InstituicaoId = empresaId,
                    NomeCompleto = colabUser.NomeCompleto,
                    EmailPessoal = colabUser.Email,
                    CargoId = cargoDevId, // Developer
                    Departamento = "Tecnologia",
                    NIF = "000000003",
                    DataNascimento = new DateTime(1995, 8, 20, 0, 0, 0, DateTimeKind.Utc),
                    DataAdmissao = DateTime.UtcNow.AddYears(-1),
                    Morada = "Viana, Luanda",
                    Telemovel = 900000003,
                    SaldoFerias = 15,
                    TipoContrato = "Termo Certo",
                    IsAtivo = true,
                    GestorId = perfilGestorId // <-- Reporta à Maria Gestora
                };

                context.Colaboradores.AddRange(perfilAdmin, perfilGestor, perfilColab);
                context.SaveChanges();

                // 9. Competências (Para testes de avaliação)
                context.Competencias.AddRange(
                    new Competencia { Nome = "Liderança", Tipo = TipoCompetencia.Comportamental, InstituicaoId = empresaId, IsAtiva = true },
                    new Competencia { Nome = "Comunicação", Tipo = TipoCompetencia.Comportamental, InstituicaoId = empresaId, IsAtiva = true },
                    new Competencia { Nome = "C# .NET Core", Tipo = TipoCompetencia.Tecnica, InstituicaoId = empresaId, IsAtiva = true },
                    new Competencia { Nome = "Angular", Tipo = TipoCompetencia.Tecnica, InstituicaoId = empresaId, IsAtiva = true },
                    new Competencia { Nome = "Gestão de Conflitos", Tipo = TipoCompetencia.Comportamental, InstituicaoId = empresaId, IsAtiva = true }
                );

                context.SaveChanges();
            }
        
    }
}