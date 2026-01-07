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
                var token = await _authService.LoginAsync(request);
                return Ok(new { token });
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

        // --- MÉTODO: Listar Utilizadores ---
        [HttpGet("users")]
        [Authorize(Roles = "GestorMaster, GestorRH")] // <-- PROTEGIDO!
        public async Task<IActionResult> GetUsers()
        {
            var users = await _authService.GetAllUsersAsync();
            return Ok(users);
        }


        // --- Métodos Auxiliares de Criptografia ---

    }
}
