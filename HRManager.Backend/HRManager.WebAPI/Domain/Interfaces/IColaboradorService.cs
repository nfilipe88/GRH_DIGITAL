using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface IColaboradorService
    {
        Task<List<ColaboradorListDto>> GetAllAsync();

        // Alterado int -> Guid
        Task<ColaboradorListDto?> GetByIdAsync(Guid id);

        // Alterado retorno para corresponder ao Service
        Task<Colaborador> CreateAsync(CriarColaboradorRequest request);

        // Alterado int -> Guid
        Task<bool> UpdateAsync(Guid id, AtualizarDadosPessoaisRequest request);

        // Alterado int -> Guid
        Task ToggleAtivoAsync(Guid id);
    }
}
