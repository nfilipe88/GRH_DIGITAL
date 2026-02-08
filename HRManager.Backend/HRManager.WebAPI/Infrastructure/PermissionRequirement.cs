using Microsoft.AspNetCore.Authorization;

namespace HRManager.WebAPI.Infrastructure
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string PermissionCode { get; }

        public PermissionRequirement(string permissionCode)
        {
            PermissionCode = permissionCode ?? throw new ArgumentNullException(nameof(permissionCode));
        }
    }
}
