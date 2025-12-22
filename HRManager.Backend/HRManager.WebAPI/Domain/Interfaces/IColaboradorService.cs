using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface IColaboradorService
    {
        Task<List<ColaboradorListDto>> GetAllAsync();
        Task<ColaboradorListDto?> GetByIdAsync(Guid id);
        Task<ColaboradorDto> CreateAsync(CriarColaboradorRequest request);
        Task<bool> UpdateAsync(Guid id, AtualizarDadosPessoaisRequest request);
        Task ToggleAtivoAsync(Guid id);
        Task DesativarColaboradorAsync(Guid colaboradorId);
        Task TransferirEquipaAsync(Guid gestorAntigoId, Guid gestorNovoId);
    }
}
