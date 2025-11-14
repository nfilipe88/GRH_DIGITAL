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

        // O serviço recebe o ficheiro de configuração (appsettings)
        public TokenService(IConfiguration config)
        {
            _config = config;
            // 1. Lê a chave secreta do appsettings.json
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]));
        }

        public string CreateToken(User user)
        {
            // 2. Definir os "Claims" - as informações que queremos guardar no token
            var claims = new List<Claim>
            {
                // O "NameId" é a forma padrão de guardar o ID do utilizador
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                // O "Email" é a forma padrão de guardar o email/username
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                // Este é o "Claim" de Cargo (Role), crucial para a Autorização
                new Claim(ClaimTypes.Role, user.Role)
            };

            // 3. Criar as credenciais de assinatura
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            // 4. Criar a descrição do token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7), // Token válido por 7 dias
                Issuer = _config["JwtSettings:Issuer"],
                Audience = _config["JwtSettings:Audience"],
                SigningCredentials = creds
            };

            // 5. Criar o "manipulador" de token
            var tokenHandler = new JwtSecurityTokenHandler();

            // 6. Criar o token
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // 7. Escrever o token como uma string
            return tokenHandler.WriteToken(token);
        }

        public string GenerateToken(User user, Guid instituicaoId, string role)
        {
            throw new NotImplementedException();
        }

        public bool isMasterTenant(string token)
        {
            throw new NotImplementedException();
        }
    }
}
