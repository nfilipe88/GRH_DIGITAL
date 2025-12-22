// Em HRManager.WebAPI/Services/TenantService.cs

using HRManager.Application.Interfaces;
using System.Security.Claims;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetInstituicaoId()
    {
        // CORREÇÃO: Mudar de "instituicao_id" para "tenantId"
        var tenantIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("tenantId")?.Value;

        if (Guid.TryParse(tenantIdClaim, out var instituicaoId))
        {
            return instituicaoId;
        }

        return Guid.Empty;
    }

    public Guid? GetTenantId()
    {
        // CORREÇÃO: Mudar de "InstituicaoId" para "tenantId"
        var tenantIdClaim = _httpContextAccessor.HttpContext?.User?
            .FindFirstValue("tenantId");

        if (string.IsNullOrEmpty(tenantIdClaim))
        {
            return null;
        }

        if (Guid.TryParse(tenantIdClaim, out Guid tenantId))
        {
            return tenantId;
        }

        return null;
    }

    // Esta propriedade já está correta
    public Guid? TenantId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            Console.WriteLine($"User authenticated: {user?.Identity?.IsAuthenticated}");

            var tenantClaim = user?.FindFirst("tenantId");
            Console.WriteLine($"Tenant claim found: {tenantClaim?.Value}");

            if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var parsedId))
            {
                return parsedId;
            }

            return null;
        }
    }

    public bool IsMasterTenant
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User.IsInRole("GestorMaster") ?? false;
        }
    }
}