// Em HRManager.WebAPI/Services/TenantService.cs

using HRManager.Application.Interfaces;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetInstituicaoId()
    {
        // 1. Obter o valor da claim 'instituicao_id' (que deve ser colocada no token JWT no login)
        var instituicaoIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("instituicao_id")?.Value;

        if (Guid.TryParse(instituicaoIdClaim, out var instituicaoId))
        {
            return instituicaoId;
        }

        // 2. Se não encontrar, lança exceção (segurança) ou retorna um Guid.Empty
        return Guid.Empty; 
    }

    public bool IsMasterTenant => _httpContextAccessor.HttpContext?.User.IsInRole("GestorMaster") ?? false;
}