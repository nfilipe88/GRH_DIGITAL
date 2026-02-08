using Microsoft.AspNetCore.Authorization;

namespace HRManager.WebAPI.Infrastructure
{
    public class PermissionAuthorizeAttribute : AuthorizeAttribute
    {
        public PermissionAuthorizeAttribute(string permissionCode)
            : base(policy: permissionCode)
        {
        }
    }
}
