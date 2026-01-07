using FluentValidation;
using HRManager.Application.Interfaces;
using HRManager.WebAPI.Constants;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HRManager.WebAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly HRManagerDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly ITenantService _tenantService;
        // 1. Injeção dos gestores do Identity
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public AuthService(
            HRManagerDbContext context, 
            ITokenService tokenService, 
            ITenantService tenantService,
            UserManager<User> userManager,
            RoleManager<Role> roleManager)
        {
            _context = context;
            _tokenService = tokenService;
            _tenantService = tenantService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<string> LoginAsync(LoginRequest request)
        {
            // Nota: Mantemos a consulta via _context aqui porque o TokenService 
            // precisa dos dados relacionados (Instituicao, Roles) carregados (Include).
            // O FindByEmailAsync padrão do Identity não traz os Includes por defeito.
            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.Instituicao)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                throw new UnauthorizedAccessException("Credenciais inválidas.");

            // 2. Validação Segura: Usamos o UserManager para verificar a password
            // Isto garante que o sistema usa o algoritmo de hash configurado no Identity
            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!passwordValid)
                throw new UnauthorizedAccessException("Credenciais inválidas.");

            if (!user.IsAtivo)
                throw new UnauthorizedAccessException("Conta inativa.");

            return _tokenService.CreateToken(user);
        }

        public async Task<string> RegisterAsync(RegisterRequest request)
        {
            // O CreateAsync já verifica duplicados, mas podemos manter esta verificação rápida
            // se quisermos uma mensagem de erro personalizada antes de tentar criar.
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                throw new ValidationException("Este endereço de email já se encontra registado.");

            var instituicaoId = _tenantService.GetInstituicaoId();

            // 3. Criação do Objeto User (SEM definir PasswordHash manualmente)
            var newUser = new User
            {
                UserName = request.Email, // O Identity exige UserName (geralmente igual ao email)
                Email = request.Email,
                NomeCompleto = request.Nome,
                InstituicaoId = instituicaoId,
                IsAtivo = true,
                SecurityStamp = Guid.NewGuid().ToString() // Boa prática para invalidar tokens antigos se mudar algo crítico
            };

            // 4. Criação Segura via UserManager
            // Ele trata do Hash da password e validações de complexidade automaticamente
            var result = await _userManager.CreateAsync(newUser, request.Password);

            if (!result.Succeeded)
            {
                // Agrupa os erros do Identity numa mensagem legível
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new ValidationException($"Erro ao criar utilizador: {errors}");
            }

            // 5. Atribuição de Role via Identity
            var roleName = string.IsNullOrEmpty(request.Role) ? RolesConstants.Colaborador : request.Role;
            
            // Verifica se a role existe
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                // Opcional: Rollback se falhar (apagar o user criado), 
                // ou garantir que as roles existem no Seeder.
                throw new InvalidOperationException($"Role '{roleName}' não encontrada.");
            }

            var roleResult = await _userManager.AddToRoleAsync(newUser, roleName);
            
            if (!roleResult.Succeeded)
            {
                throw new ValidationException("Erro ao associar perfil ao utilizador.");
            }

            // Precisamos carregar a Instituição para o TokenService, 
            // pois o objeto 'newUser' ainda não tem a propriedade de navegação preenchida.
            // Uma forma simples é reatribuir o ID ou carregar do contexto, 
            // mas como acabamos de criar, sabemos o ID.
            if (newUser.Instituicao == null && instituicaoId != Guid.Empty)
            {
                newUser.Instituicao = await _context.Instituicoes.FindAsync(instituicaoId);
            }

            // Se o TokenService precisar da Role na lista UserRoles para gerar claims:
            // O AddToRoleAsync salva na BD, mas não atualiza a lista em memória 'newUser.UserRoles'.
            // Para garantir o token correto, podemos buscar o user completo ou construir manualmente para o token.
            // Solução robusta: buscar o utilizador completo agora.
            var userForToken = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.Instituicao)
                .FirstAsync(u => u.Id == newUser.Id);

            return _tokenService.CreateToken(userForToken);
        }

        public async Task<UserDetailsDto> GetCurrentUserAsync(string email)
        {
            // Mantemos a leitura via Context para performance (Includes), 
            // já que é apenas leitura.
            var user = await _context.Users
                .Include(u => u.Instituicao)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null) throw new KeyNotFoundException("Utilizador não encontrado.");

            var roleName = user.UserRoles.FirstOrDefault()?.Role?.Name ?? "N/A";

            return new UserDetailsDto
            {
                Id = user.Id,
                NomeCompleto = user.NomeCompleto,
                Email = user.Email ?? "",
                Cargo = roleName, 
                InstituicaoNome = user.Instituicao?.Nome ?? "N/A",
                IsAtivo = user.IsAtivo
            };
        }

        public async Task<List<UserListDto>> GetAllUsersAsync()
        {
            var tenantId = _tenantService.GetTenantId();
            
            var query = _context.Users.AsQueryable();
            if (tenantId != Guid.Empty && tenantId != null)
            {
                query = query.Where(u => u.InstituicaoId == tenantId);
            }

            return await query
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Select(u => new UserListDto
                {
                    Id = u.Id,
                    Nome = u.NomeCompleto,
                    Email = u.Email ?? "",
                    Role = u.UserRoles.FirstOrDefault() != null && u.UserRoles.FirstOrDefault()!.Role != null
                        ? u.UserRoles.FirstOrDefault()!.Role!.Name ?? "N/A"
                        : "N/A"
                })
                .ToListAsync();
        }
    }
}
