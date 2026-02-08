namespace HRManager.WebAPI.Constants
{
    public static class RolesConstants
    {
        // Roles Individuais
        public const string Admin = "Admin";
        public const string GestorMaster = "GestorMaster";
        public const string GestorRH = "GestorRH";
        public const string Colaborador = "Colaborador";

        // Combinações de Acesso (Policies simplificadas)
        
        // Acesso Administrativo (Master ou RH)
        public const string AdminAccess = GestorMaster + "," + GestorRH;

        // Acesso Geral (Todos os perfis autenticados que tenham role)
        public const string GeneralAccess = GestorMaster + "," + GestorRH + "," + Colaborador;
    }
}