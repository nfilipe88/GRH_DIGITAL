using HRManager.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManager.WebAPI.Data
{
    public static class PermissionsSeeder
    {
        public static List<Permission> GetInitialPermissions()
        {
            var permissions = new List<Permission>();

            // Módulo: Gestão de Utilizadores
            permissions.AddRange(new[]
            {
            new Permission { Code = "USERS_VIEW", Name = "Ver Utilizadores", Module = "Utilizadores", Category = "Leitura" },
            new Permission { Code = "USERS_CREATE", Name = "Criar Utilizador", Module = "Utilizadores", Category = "Escrita" },
            new Permission { Code = "USERS_EDIT", Name = "Editar Utilizador", Module = "Utilizadores", Category = "Escrita" },
            new Permission { Code = "USERS_DELETE", Name = "Eliminar Utilizador", Module = "Utilizadores", Category = "Escrita" },
            new Permission { Code = "USERS_CHANGE_ROLE", Name = "Alterar Role", Module = "Utilizadores", Category = "Administração" },
        });

            // Módulo: Gestão de Instituições
            permissions.AddRange(new[]
            {
            new Permission { Code = "INSTITUTIONS_VIEW", Name = "Ver Instituições", Module = "Instituições", Category = "Leitura" },
            new Permission { Code = "INSTITUTIONS_CREATE", Name = "Criar Instituição", Module = "Instituições", Category = "Escrita" },
            new Permission { Code = "INSTITUTIONS_EDIT", Name = "Editar Instituição", Module = "Instituições", Category = "Escrita" },
            new Permission { Code = "INSTITUTIONS_DELETE", Name = "Eliminar Instituição", Module = "Instituições", Category = "Escrita" },
            new Permission { Code = "INSTITUTIONS_TOGGLE", Name = "Ativar/Inativar", Module = "Instituições", Category = "Administração" },
        });

            // Módulo: Gestão de Roles
            permissions.AddRange(new[]
            {
            new Permission { Code = "ROLES_VIEW", Name = "Ver Roles", Module = "Roles", Category = "Leitura" },
            new Permission { Code = "ROLES_CREATE", Name = "Criar Role", Module = "Roles", Category = "Escrita" },
            new Permission { Code = "ROLES_EDIT", Name = "Editar Role", Module = "Roles", Category = "Escrita" },
            new Permission { Code = "ROLES_DELETE", Name = "Eliminar Role", Module = "Roles", Category = "Escrita" },
            new Permission { Code = "ROLES_MANAGE_PERMISSIONS", Name = "Gerir Permissões", Module = "Roles", Category = "Administração" },
        });

            // Módulo: Permissões
            permissions.AddRange(new[]
            {
            new Permission { Code = "PERMISSIONS_VIEW", Name = "Ver Permissões", Module = "Permissões", Category = "Leitura" },
            new Permission { Code = "PERMISSIONS_MANAGE", Name = "Gerir Permissões", Module = "Permissões", Category = "Administração" },
        });

            // Módulo: Ausências
            permissions.AddRange(new[]
            {
            new Permission { Code = "ABSENCES_VIEW_ALL", Name = "Ver Todas Ausências", Module = "Ausências", Category = "Leitura" },
            new Permission { Code = "ABSENCES_VIEW_TEAM", Name = "Ver Ausências da Equipa", Module = "Ausências", Category = "Leitura" },
            new Permission { Code = "ABSENCES_APPROVE", Name = "Aprovar Ausências", Module = "Ausências", Category = "Administração" },
        });

            // Atribuir IDs
            foreach (var permission in permissions)
            {
                permission.Id = Guid.NewGuid();
                permission.IsActive = true;
                permission.CreatedAt = DateTime.UtcNow;
            }

            return permissions;
        }

        public static async Task SeedAsync(HRManagerDbContext context)
        {
            if (!await context.Permissions.AnyAsync())
            {
                var permissions = GetInitialPermissions();
                await context.Permissions.AddRangeAsync(permissions);
                await context.SaveChangesAsync();

                // Criar roles padrão com permissões
                await CreateDefaultRolesAsync(context, permissions);
            }
        }

        private static async Task CreateDefaultRolesAsync(HRManagerDbContext context, List<Permission> permissions)
        {
            // Role: GestorMaster (tem TODAS as permissões)
            var gestorMasterRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Name == "GestorMaster");

            if (gestorMasterRole == null)
            {
                gestorMasterRole = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "GestorMaster",
                    NormalizedName = "GESTORMASTER",
                    Description = "Administrador com acesso total ao sistema",
                    IsSystemRole = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Roles.Add(gestorMasterRole);
                await context.SaveChangesAsync();

                // Atribuir TODAS as permissões ao GestorMaster
                foreach (var permission in permissions)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = gestorMasterRole.Id,
                        PermissionId = permission.Id
                    });
                }
            }

            // Role: GestorRH (tem permissões específicas)
            var gestorRHRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Name == "GestorRH");

            if (gestorRHRole == null)
            {
                gestorRHRole = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "GestorRH",
                    NormalizedName = "GESTORRH",
                    Description = "Gestor de Recursos Humanos",
                    IsSystemRole = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Roles.Add(gestorRHRole);
                await context.SaveChangesAsync();

                // Atribuir permissões específicas ao GestorRH
                var gestorRHPermissions = permissions
                    .Where(p => p.Module == "Utilizadores" ||
                               p.Module == "Ausências" ||
                               p.Code == "INSTITUTIONS_VIEW")
                    .ToList();

                foreach (var permission in gestorRHPermissions)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = gestorRHRole.Id,
                        PermissionId = permission.Id
                    });
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
