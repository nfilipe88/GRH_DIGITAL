namespace HRManager.WebAPI.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalInstituicoes { get; set; }
        public int TotalColaboradores { get; set; }
        public int TotalColaboradoresAtivos { get; set; }
        public int TotalUtilizadores { get; set; }
        public int AusenciasPendentes { get; set; }
        public int NovasAdmissoesMes { get; set; }

        // Útil para gráficos de tarte (Pie Chart)
        public Dictionary<string, int> ColaboradoresPorDepartamento { get; set; } = new();

        // Útil para lista rápida "Quem está de férias hoje?"
        public List<string> ColaboradoresAusentesHoje { get; set; } = new();
    }
}
