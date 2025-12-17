namespace HRManager.WebAPI.DTOs
{
    public class DashboardStatsDto
    {
        // --- DADOS PARA GESTORES ---
        public int TotalColaboradores { get; set; }
        public int TotalAusenciasAtivas { get; set; }
        public int AvaliacoesEmAndamento { get; set; }
        public List<DepartamentoStatDto> ColaboradoresPorDepartamento { get; set; } = new();

        // --- DADOS PARA COLABORADORES (NOVO) ---
        public int MeusDiasFeriasDisponiveis { get; set; }
        public string? ProximaAvaliacaoEstado { get; set; } // Ex: "Não Iniciada", "Autoavaliação Pendente"
        public int MinhasDeclaracoesPendentes { get; set; }

        // Flag para facilitar o frontend (opcional, mas útil)
        public bool IsVisaoGestor { get; set; }
    }
}
