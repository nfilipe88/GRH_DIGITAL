using HRManager.WebAPI.DTOs;

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<string> LoginAsync(LoginRequest request);
        Task<UserListDto> RegisterAsync(RegisterRequest request);
        Task<UserDetailsDto> GetCurrentUserAsync(string email);
        Task<List<UserListDto>> GetAllUsersAsync();
    }
}
