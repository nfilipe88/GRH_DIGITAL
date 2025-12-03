namespace HRManager.WebAPI.Domain.enums
{
    public enum EstadoAvaliacao
    {
        NaoIniciada,        // Criada mas ninguém tocou
        AutoAvaliacao,      // Colaborador está a preencher
        EmAndamento,
        AnaliseGestor,      // Colaborador submeteu, Gestor a preencher
        Finalizada          // Gestor fechou
    }
}
