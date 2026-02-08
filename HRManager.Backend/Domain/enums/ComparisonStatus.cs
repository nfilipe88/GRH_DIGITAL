namespace HRManager.Domain.enums
{
    public enum ComparisonStatus
    {
        Match,      // Presente em ambos
        Missing,    // No template, não na role
        Extra       // Na role, não no template
    }
}
