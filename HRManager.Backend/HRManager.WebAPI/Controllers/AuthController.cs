using FluentValidation;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // ---
        // GET: Obter dados do utilizador atual (Me)
        // ---
        [HttpGet("me")]
        [Authorize] // <-- Exige token válido
        public async Task<IActionResult> GetMe()
        {
            // Extrai o email do token (ClaimTypes.Email ou "email")
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");

            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { message = "Token inválido." });

            try
            {
                var me = await _authService.GetCurrentUserAsync(email);
                return Ok(me);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Utilizador não encontrado." });
            }
        }

        [HttpPost("register")]
        [Authorize(Roles = "GestorMaster, GestorRH")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // CORREÇÃO: RegisterAsync agora devolve uma string (Token)
                var token = await _authService.RegisterAsync(request);

                // Retornamos OK com o token gerado
                return Ok(new { token });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                await _authService.ChangePasswordAsync(request);
                return Ok(new { message = "Password alterada com sucesso! Por favor, faça login novamente." });
            }
            catch (KeyNotFoundException)
            {
                // Por segurança, não revelamos se o email existe ou não, ou retornamos BadRequest genérico
                return BadRequest(new { message = "Dados inválidos." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocorreu um erro interno."+ ex.Message });
            }
        }

        // --- MÉTODO: Listar Utilizadores Paginado ---
        [HttpGet("users")]
        [Authorize(Roles = "GestorMaster, GestorRH")]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // Validação básica para evitar erros
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Proteção contra pedidos gigantes

            var pagedResult = await _authService.GetAllUsersAsync(page, pageSize);
            return Ok(pagedResult);
        }

        [HttpGet("permissions")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserPermissions()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                // Buscar o usuário com suas roles
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));

                if (user == null)
                    return NotFound(new { message = "Utilizador não encontrado" });

                // Extrair códigos de permissão únicos
                var permissionCodes = user.UserRoles
                    .SelectMany(ur => ur.Role.RolePermissions)
                    .Select(rp => rp.Permission.Code)
                    .Distinct()
                    .ToList();

                return Ok(permissionCodes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao obter permissões", error = ex.Message });
            }
        }
        // --- Métodos Auxiliares de Criptografia ---

    }
}
