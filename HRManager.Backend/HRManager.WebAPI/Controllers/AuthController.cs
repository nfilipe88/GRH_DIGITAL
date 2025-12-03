using FluentValidation;
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
                var user = await _authService.RegisterAsync(request);
                return CreatedAtAction(nameof(Login), new { email = user.Email }, user);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Validação automática do FluentValidation ocorre aqui
            try
            {
                var token = await _authService.LoginAsync(request);
                return Ok(new { token });
            }
            catch (ValidationException ex)
            {
                return Unauthorized(new { message = ex.Message }); // 401 para falha de login
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
