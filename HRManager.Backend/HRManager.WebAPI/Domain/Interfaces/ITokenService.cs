using HRManager.WebAPI.Models;

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user);
        string GenerateToken(User user, Guid instituicaoId, string role);
        bool isMasterTenant(string token);
    }
}