namespace HRManager.WebAPI.Constants
{
    public class RolesConstants
    {
        public const string GestorMaster = "GestorMaster";
        public const string GestorRH = "GestorRH";
        public const string Colaborador = "Colaborador";

        // Combinações úteis para os atributos [Authorize]
        public const string ApenasGestores = GestorMaster + "," + GestorRH;
        public const string Todos = GestorMaster + "," + GestorRH + "," + Colaborador;
    }
}
