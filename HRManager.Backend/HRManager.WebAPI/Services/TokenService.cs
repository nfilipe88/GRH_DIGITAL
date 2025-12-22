using HRManager.WebAPI.Models;
using HRManager.WebAPI.Domain.Interfaces;
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

        public string CreateToken(User user)
        {
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""), // Garante string vazia se nulo
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
        };

            // CORREÇÃO: User.InstituicaoId é um Guid, não Guid?. Verificamos se não é Empty.
            if (user.InstituicaoId != Guid.Empty)
            {
                // CORREÇÃO: Removemos .Value (pois não é anulável)
                claims.Add(new Claim("tenantId", user.InstituicaoId.ToString()));
            }

            if (user.UserRoles != null)
            {
                foreach (var ur in user.UserRoles)
                {
                    if (ur.Role != null)
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
