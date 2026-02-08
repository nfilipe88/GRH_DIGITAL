using HRManager.WebAPI.DTOs;

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<string> RegisterAsync(RegisterRequest request);
        Task<UserDetailsDto> GetCurrentUserAsync(string email);
        Task<PagedResult<UserListDto>> GetAllUsersAsync(int pageNumber, int pageSize);
        Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
    }
}
