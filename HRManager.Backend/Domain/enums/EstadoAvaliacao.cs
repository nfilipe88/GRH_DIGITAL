namespace HRManager.WebAPI.Domain.enums
{
    public enum EstadoAvaliacao
    {
        AnaliseGestor,      // Colaborador submeteu, Gestor a preencher
        AutoAvaliacao,      // Colaborador está a preencher
        Cancelada,
        EmAndamento,
        NaoIniciada,        // Criada mas ninguém tocou
        Finalizada          // Gestor fechou
    }
}
