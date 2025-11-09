using HRManager.WebAPI.Domain.Entities;

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user, Guid instituicaoId, string role);
        bool isMasterTenant(string token);
    }
}