interface MenuItem {
  label: string;
  icon: string;
  route: string;
  roles: string[]; // Define quem pode ver este item ('*' = todos)
}
