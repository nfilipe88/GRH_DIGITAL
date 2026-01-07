using HRManager.WebAPI.DTOs;

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface IDashboardService
    {
        //Task<DashboardStatsDto> GetStatsAsync(Guid? instituicaoId = null);//EM USO
        //Task<DashboardStatsDto> GetStatsAsync(Guid userId, IEnumerable<string> roles);
        //Task<DashboardStatsDto> GetStatsAsync();
        Task<DashboardStatsDto> GetDashboardStatsAsync(string email, List<string> roles);
    }
}
