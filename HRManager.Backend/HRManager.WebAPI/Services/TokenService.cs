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

            // --- INÍCIO DA NOSSA ALTERAÇÃO ---
            // Adiciona a InstituicaoId ao token, se ela existir (para GestoresRH)
            if (user.InstituicaoId.HasValue)
            {
                // "InstituicaoId" é o nome da nossa claim personalizada
                claims.Add(new Claim("InstituicaoId", user.InstituicaoId.Value.ToString()));
            }
            // --- FIM DA NOSSA ALTERAÇÃO ---

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
            // 1. Definir os Claims (Dados do Utilizador no Token)
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, role) // Usa o role passado como argumento
            };

            // 2. Adicionar Instituição se existir (e não for Empty)
            if (instituicaoId != Guid.Empty)
            {
                claims.Add(new Claim("InstituicaoId", instituicaoId.ToString()));
                // Se a instituição estiver carregada no objeto user, adicionamos o nome
                if (user.Instituicao != null)
                {
                    claims.Add(new Claim("InstituicaoNome", user.Instituicao.Nome));
                }
            }

            // 3. Criar credenciais
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            // 4. Configurar o Token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7), // Use UtcNow para evitar problemas de fuso horário
                Issuer = _config["JwtSettings:Issuer"],
                Audience = _config["JwtSettings:Audience"],
                SigningCredentials = creds
            };

            // 5. Gerar a String
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
