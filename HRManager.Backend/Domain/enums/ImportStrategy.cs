namespace HRManager.WebAPI.Domain.enums
{
    public enum ImportStrategy
    {
        Merge,      // Mantém existentes, adiciona novos
        Replace,    // Substitui completamente
        Update      // Apenas atualiza existentes
    }
}
