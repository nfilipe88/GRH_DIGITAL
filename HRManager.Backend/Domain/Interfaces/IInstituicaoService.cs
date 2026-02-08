using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface IInstituicaoService
    {
        Task<List<InstituicaoListDto>> GetAllAsync();
        Task<Instituicao?> GetByIdAsync(Guid id);
        Task<Instituicao?> GetBySlugAsync(string slug);
        Task<Instituicao> CreateAsync(CriarInstituicaoRequest request);
        Task<Instituicao> UpdateAsync(Guid id, AtualizarInstituicaoRequest request);
        // Task DeleteAsync(Guid id); // Geralmente apenas inativamos (Soft Delete)
    }
}
