// Em HRManager.Domain/Interfaces/IHaveTenant.cs

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface IHaveTenant
    {
        // Chave estrangeira que liga o registo à Instituição
        Guid InstituicaoId { get; set; }
    }
}