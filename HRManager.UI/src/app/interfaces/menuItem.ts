interface MenuItem {
  path: string;
  label: string;
  icon?: string;
  requiredPermissions: string[];
  requiredRoles?: string[];
  children?: MenuItem[];
}
