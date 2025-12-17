using FluentValidation;
using HRManager.Application.Interfaces;
using HRManager.WebAPI.Constants;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManager.WebAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly HRManagerDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly ITenantService _tenantService;

        public AuthService(HRManagerDbContext context, ITokenService tokenService, ITenantService tenantService)
        {
            _context = context;
            _tokenService = tokenService;
            _tenantService = tenantService;
        }

        public async Task<string> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.Instituicao)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Credenciais inválidas.");

            if (!user.IsAtivo)
                throw new UnauthorizedAccessException("Conta inativa.");

            return _tokenService.CreateToken(user);
        }

        public async Task<string> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                throw new ValidationException("Este endereço de email já se encontra registado.");

            var instituicaoId = _tenantService.GetInstituicaoId();

            var newUser = new User
            {
                NomeCompleto = request.Nome,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                InstituicaoId = instituicaoId,
                IsAtivo = true
            };

            var roleName = string.IsNullOrEmpty(request.Role) ? RolesConstants.Colaborador : request.Role;
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            
            if (role == null) throw new InvalidOperationException($"Role '{roleName}' não encontrada.");

            newUser.UserRoles.Add(new UserRole { Role = role, User = newUser });

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return _tokenService.CreateToken(newUser);
        }

        public async Task<UserDetailsDto> GetCurrentUserAsync(string email)
        {
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
                Email = user.Email,
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
                    Email = u.Email,
                    // CORREÇÃO AQUI: Verificação de nulos em cadeia (CS8602)
                    Role = u.UserRoles.FirstOrDefault() != null 
                           ? (u.UserRoles.FirstOrDefault()!.Role != null ? u.UserRoles.FirstOrDefault()!.Role.Name : "N/A") 
                           : "N/A"
                })
                .ToListAsync();
        }
    }
}
