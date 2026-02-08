using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HRManager.WebAPI.Models;
using HRManager.WebAPI.Constants;

namespace HRManager.WebAPI.Data
{
    public static class DbSeeder
    {
        public static async Task Seed(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<HRManagerDbContext>();
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<Role>>();

            // Garantir que a base de dados está criada
            await context.Database.MigrateAsync();

            // 1. CRIAR ROLES SE NÃO EXISTIREM
            await EnsureRolesAsync(roleManager);

            // 2. CRIAR INSTITUIÇÃO MASTER (OPCIONAL)
            var instituicaoMaster = await EnsureInstituicaoMasterAsync(context);

            // 3. CRIAR UTILIZADOR GESTOR MASTER
            await EnsureGestorMasterAsync(userManager, instituicaoMaster);
        }

        private static async Task EnsureRolesAsync(RoleManager<Role> roleManager)
        {
            string[] roles = { RolesConstants.GestorMaster, RolesConstants.GestorRH, RolesConstants.Colaborador };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new Role { Name = roleName });
                }
            }
        }

        private static async Task<Instituicao?> EnsureInstituicaoMasterAsync(HRManagerDbContext context)
        {
            // Verificar se já existe uma instituição master
            var instituicaoMaster = await context.Instituicoes
                .FirstOrDefaultAsync(i => i.IdentificadorUnico == "MASTER-CORP");

            if (instituicaoMaster == null)
            {
                instituicaoMaster = new Instituicao
                {
                    Id = Guid.NewGuid(),
                    Nome = "Master Corporation",
                    IdentificadorUnico = "MASTER-CORP",
                    NIF = "999999999",
                    Endereco = "Sede Principal",
                    Telemovel = 999999999,
                    EmailContato = "admin@mastercorp.com",
                    DataCriacao = DateTime.UtcNow,
                    IsAtiva = true
                };

                context.Instituicoes.Add(instituicaoMaster);
                await context.SaveChangesAsync();
            }

            return instituicaoMaster;
        }

        private static async Task EnsureGestorMasterAsync(UserManager<User> userManager, Instituicao? instituicaoMaster)
        {
            // Email do Gestor Master
            const string masterEmail = "admin@master.com";

            var gestorMaster = await userManager.FindByEmailAsync(masterEmail);

            if (gestorMaster == null)
            {
                gestorMaster = new User
                {
                    UserName = masterEmail,
                    Email = masterEmail,
                    NomeCompleto = "Administrador Master",
                    InstituicaoId = instituicaoMaster?.Id ?? Guid.Empty,
                    IsAtivo = true,
                    MustChangePassword = false,
                    EmailConfirmed = true
                };

                // Criar utilizador com password
                var result = await userManager.CreateAsync(gestorMaster, "Master@12345!");

                if (result.Succeeded)
                {
                    // Atribuir role GestorMaster
                    await userManager.AddToRoleAsync(gestorMaster, RolesConstants.GestorMaster);
                }
            }
        }
    }
}