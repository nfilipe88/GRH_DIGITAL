using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HRManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly HRManagerDbContext _context;
        private readonly ITokenService _tokenService; // Usamos o serviço que já existe

        public AuthController(HRManagerDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // ---
        // GET: Obter dados do utilizador atual (Me)
        // ---
        [HttpGet("me")]
        [Authorize] // <-- Exige token válido
        public async Task<IActionResult> GetMe()
        {
            // 1. Ler o ID do utilizador a partir do Token (Claim: NameIdentifier)
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            int userId = int.Parse(userIdStr);

            // 2. Buscar dados na BD
            var userDto = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new UserDetailsDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    Role = u.Role,
                    // Buscar o nome da instituição (subquery simples)
                    NomeInstituicao = _context.Instituicoes
                                        .Where(i => i.Id == u.InstituicaoId)
                                        .Select(i => i.Nome)
                                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (userDto == null) return NotFound(new { message = "Utilizador não encontrado." });

            return Ok(userDto);
        }

        [HttpPost("register")]
        [Authorize(Roles = "GestorMaster, GestorRH")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "Este email já está a ser utilizado." });
            }

            // 1. Criar o Hash e Salt da senha
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = request.Role,
                InstituicaoId = request.InstituicaoId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Utilizador registado com sucesso." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Credenciais inválidas." });
            }

            // 2. Verificar o Hash da senha
            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return Unauthorized(new { message = "Credenciais inválidas." });
            }

            // 3. Gerar o Token JWT
            var token = _tokenService.CreateToken(user);

            // Enviamos o token para o frontend
            return Ok(new
            {
                token = token,
                email = user.Email,
                role = user.Role
            });
        }

        // --- MÉTODO: Listar Utilizadores ---
        [HttpGet("users")]
        [Authorize(Roles = "GestorMaster, GestorRH")] // <-- PROTEGIDO!
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new UserListDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    Role = u.Role,
                    // Faz um join opcional para buscar o nome da instituição
                    NomeInstituicao = _context.Instituicoes
                                        .Where(i => i.Id == u.InstituicaoId)
                                        .Select(i => i.Nome)
                                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(users);
        }


        // --- Métodos Auxiliares de Criptografia ---

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
