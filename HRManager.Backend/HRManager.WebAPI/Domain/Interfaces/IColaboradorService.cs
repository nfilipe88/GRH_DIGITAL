using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface IColaboradorService
    {
        //Task<IEnumerable<ColaboradorListDto>> ListarTodosAsync();
        //Task<Colaborador> ObterPorIdAsync(int id);
        //Task<Colaborador> CriarColaboradorAsync(CriarColaboradorRequest request);
        //Task AtualizarColaboradorAsync(int id, CriarColaboradorRequest request);
        Task<List<ColaboradorListDto>> GetAllAsync(Guid? instituicaoId = null); // Opcional para Master
        Task<UserDetailsDto?> GetByIdAsync(int id);
        Task<UserDetailsDto> CreateAsync(CriarColaboradorRequest request);
        Task<UserDetailsDto> UpdateAsync(int id, AtualizarDadosPessoaisRequest request);
        Task ToggleAtivoAsync(int id); // Ativar/Desativar
    }
}
