using System.Security.Claims;

namespace HRManager.WebAPI.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Obtém o ID do utilizador autenticado (Claim: NameId)
        /// </summary>
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(id, out var userId) ? userId : Guid.Empty;
        }

        /// <summary>
        /// Obtém o ID da Instituição do token (Claim: InstituicaoId)
        /// Retorna NULL se não existir (ex: GestorMaster ou Admin global)
        /// </summary>
        public static Guid? GetTenantId(this ClaimsPrincipal user)
        {
            var tenantId = user.FindFirstValue("InstituicaoId");
            return Guid.TryParse(tenantId, out var id) ? id : null;
        }

        /// <summary>
        /// Verifica se o utilizador tem um papel específico
        /// </summary>
        public static bool IsRole(this ClaimsPrincipal user, string role)
        {
            return user.IsInRole(role);
        }
    }
}
