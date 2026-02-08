namespace HRManager.Domain.enums
{
    public enum AuditAction
    {
        None = 0,
        Created = 1,
        Updated = 2,
        Deleted = 3,
        PermissionAssigned = 4,
        PermissionRemoved = 5,
        RoleAssigned = 6,
        RoleRemoved = 7
    }
}
