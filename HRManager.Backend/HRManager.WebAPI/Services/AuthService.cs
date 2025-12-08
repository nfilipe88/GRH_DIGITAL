using FluentValidation;
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

        public AuthService(HRManagerDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<string> LoginAsync(LoginRequest request)
        {
            // 1. Buscar utilizador
            var user = await _context.Users
                .Include(u => u.Instituicao) // Importante incluir para o Token
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                throw new ValidationException("Email ou senha inválidos.");

            // 2. Verificar Senha (CORREÇÃO: BCrypt Verify com strings)
            // Agora user.PasswordHash é string, então funciona.
            bool senhaValida = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!senhaValida)
                throw new ValidationException("Email ou senha inválidos.");

            if (!user.IsAtivo)
                throw new ValidationException("Conta inativa. Contacte o administrador.");

            // 3. Gerar Token (CORREÇÃO: Passar InstituicaoId)
            // Se InstituicaoId for null, passamos Guid.Empty ou tratamos conforme a regra do TokenService
            var instituicaoId = user.InstituicaoId ?? Guid.Empty;

            return _tokenService.GenerateToken(user, instituicaoId, user.Role);
        }

        public async Task<UserListDto> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                throw new ValidationException("Este email já está registado.");

            // CORREÇÃO: BCrypt Hash retorna string, que agora cabe no modelo User
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new User
            {
                Nome = request.Nome,
                Email = request.Email,
                PasswordHash = passwordHash,
                Role = request.Role,
                IsAtivo = true
                // InstituicaoId: Lógica de atribuição depende do contexto (quem está a criar?)
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return new UserListDto
            {
                Id = newUser.Id,
                Nome = newUser.Nome,
                Email = newUser.Email,
                Role = newUser.Role
            };
        }

        // --- MÉTODOS RESTAURADOS ---

        public async Task<UserDetailsDto> GetCurrentUserAsync(string email)
        {
            var user = await _context.Users
                .Include(u => u.Instituicao)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null) throw new KeyNotFoundException("Utilizador não encontrado.");

            return new UserDetailsDto
            {
                Id = user.Id,
                NomeCompleto = user.Nome,
                Email = user.Email,
                Cargo = user.Role, // Mapear Role para Cargo ou ter campo separado
                InstituicaoNome = user.Instituicao?.Nome ?? "N/A",
                IsAtivo = user.IsAtivo
                // Adicione outros campos conforme necessário no DTO
            };
        }

        public async Task<List<UserListDto>> GetAllUsersAsync()
        {
            // Aqui pode adicionar filtros de segurança (ex: só ver users da minha instituição)
            return await _context.Users
                .Select(u => new UserListDto
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToListAsync();
        }
    }
}
