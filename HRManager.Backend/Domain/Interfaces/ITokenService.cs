using HRManager.WebAPI.Models;

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user, IList<string> roles);
        string CreateToken(User user);
        bool isMasterTenant(string token);
    }
}