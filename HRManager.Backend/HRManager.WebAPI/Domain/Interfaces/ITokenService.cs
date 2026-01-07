using HRManager.WebAPI.Models;

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user);
        bool isMasterTenant(string token);
    }
}