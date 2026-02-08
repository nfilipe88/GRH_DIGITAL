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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(HRManagerDbContext context, ITokenService tokenService, ITenantService tenantService,
            UserManager<User> userManager, RoleManager<Role> roleManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _tokenService = tokenService;
            _tenantService = tenantService;
            _userManager = userManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            // 1. Validar utilizador
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Por segurança, mensagem genérica
                throw new UnauthorizedAccessException("Email ou password inválidos.");
            }

            if (!user.IsAtivo)
            {
                throw new UnauthorizedAccessException("A conta está desativada.");
            }

            // 2. Validar Password
            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                throw new UnauthorizedAccessException("Email ou password inválidos.");
            }

            // 3. Gerar Token (Gera sempre, mas o frontend decide o que fazer com ele)
            // Nota: Se quiseres ser MUITO restritivo, podes gerar um token especial temporário aqui 
            // se MustChangePassword for true, que só dá acesso à rota de mudar pass.
            // Para simplificar agora, usamos o token normal.
            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateToken(user, roles);

            // 4. Retornar Resposta Completa
            return new LoginResponse
            {
                Token = token,
                MustChangePassword = user.MustChangePassword, // <--- O CAMPO MÁGICO
                NomeUser = user.NomeCompleto,
                Email = user.Email ?? ""
            };
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
        {
            // 1. Validações básicas
            if (request.NovaPassword != request.ConfirmarNovaPassword)
                throw new ArgumentException("A nova password e a confirmação não coincidem.");

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new KeyNotFoundException("Utilizador não encontrado.");

            // 2. Tentar mudar a password (O Identity verifica se a 'PasswordAtual' está correta)
            var result = await _userManager.ChangePasswordAsync(user, request.PasswordAtual, request.NovaPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ArgumentException($"Não foi possível alterar a password: {errors}");
            }

            // 3. Remover a obrigatoriedade de mudança (Flag)
            if (user.MustChangePassword)
            {
                user.MustChangePassword = false;
                await _userManager.UpdateAsync(user);
            }

            return true;
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

        public async Task<PagedResult<UserListDto>> GetAllUsersAsync(int page, int pageSize)
        {
            // 1. Iniciar a query IGNORANDO os filtros globais (Tenant)
            var query = _context.Users
                .IgnoreQueryFilters() // <--- A CORREÇÃO PRINCIPAL
                .Include(u => u.Instituicao)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role) // Incluir Roles para evitar queries N+1
                .AsQueryable();

            // 2. Verificar se é GestorMaster
            var user = _httpContextAccessor.HttpContext?.User;
            bool isMaster = user?.IsInRole(RolesConstants.GestorMaster) ?? false;

            // 3. Se NÃO for master, aplicamos o filtro de tenant MANUALMENTE
            if (!isMaster)
            {
                var tenantId = _tenantService.GetInstituicaoId();
                query = query.Where(u => u.InstituicaoId == tenantId);
            }

            // 4. Paginação e Projeção (Manteve-se igual)
            var totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.NomeCompleto) // Boa prática: Ordenar sempre paginações
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListDto
                {
                    Id = u.Id,
                    NomeCompleto = u.NomeCompleto,
                    Email = u.Email ?? "",
                    // Obtém o nome da Role ou "Sem Perfil"
                    Role = u.UserRoles.Select(ur => ur.Role.Name).FirstOrDefault() ?? "Sem Perfil",
                    InstituicaoNome = u.Instituicao != null ? u.Instituicao.Nome : "N/A",
                    IsAtivo = u.IsAtivo
                })
                .ToListAsync();

            return new PagedResult<UserListDto>
            {
                Items = users,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }
    }
}