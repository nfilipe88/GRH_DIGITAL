using HRManager.WebAPI.Constants;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HRManager.WebAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;

        public TokenService(IConfiguration config)
        {
            _config = config;
            var keyString = _config["JwtSettings:Key"] ?? throw new InvalidOperationException("JwtSettings:Key não configurada.");
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        }

        public string GenerateToken(User user, IList<string> roles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // Lê a chave secreta do appsettings.json
            var key = Encoding.ASCII.GetBytes(_config["JwtSettings:Key"] ?? throw new InvalidOperationException("JwtSettings:Key não configurada."));

            // Define as Claims (Dados dentro do Token)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                // Importante: Guardar o TenantId no token ajuda a filtrar dados automaticamente depois
                new Claim("tenantId", user.InstituicaoId.ToString())
            };

            // Adiciona as Roles (Perfis) ao Token
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Configura o Token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8), // O token expira em 8 horas
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _config["JwtSettings:Issuer"],
                Audience = _config["JwtSettings:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
            };

            // IMPORTANTE: Se for GestorMaster, NÃO adicionar tenantId
            // Isso permite visão total
            bool isGestorMaster = user.UserRoles?.Any(ur => ur.Role?.Name == RolesConstants.GestorMaster) ?? false;

            if (!isGestorMaster && user.InstituicaoId != Guid.Empty)
            {
                // Para outros utilizadores, adicionar tenantId
                claims.Add(new Claim("tenantId", user.InstituicaoId.ToString()));
                claims.Add(new Claim("InstituicaoId", user.InstituicaoId.ToString()));

                if (user.Instituicao != null)
                {
                    claims.Add(new Claim("InstituicaoNome", user.Instituicao.Nome));
                }
            }
            else if (isGestorMaster)
            {
                // Para GestorMaster, adicionar uma flag especial
                claims.Add(new Claim("IsMaster", "true"));
            }

            // Adicionar roles
            if (user.UserRoles != null)
            {
                foreach (var ur in user.UserRoles)
                {
                    if (ur.Role != null && ur.Role.Name != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, ur.Role.Name));
                    }
                }
            }

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = _config["JwtSettings:Issuer"],
                Audience = _config["JwtSettings:Audience"],
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        // Implementação básica para evitar outro erro de NotImplemented
        public bool isMasterTenant(string token)
        {
            // Lógica simples: ler o token e ver se tem a role "GestorMaster"
            // (Para já retornamos false se não for crítico, ou implementamos a leitura)
            var handler = new JwtSecurityTokenHandler();
            if (handler.CanReadToken(token))
            {
                var jwtToken = handler.ReadJwtToken(token);
                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == ClaimTypes.Role);
                return roleClaim?.Value == "GestorMaster";
            }
            return false;
        }
    }
}
