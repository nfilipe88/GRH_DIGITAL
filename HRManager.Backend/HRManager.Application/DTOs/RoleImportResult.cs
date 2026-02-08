namespace HRManager.WebAPI.DTOs
{
    public class RoleImportResult
    {
        public int TotalProcessed { get; set; }
        public int Created { get; set; }
        public int Updated { get; set; }
        public int Skipped { get; set; }
        public int Failed { get; set; }
        public List<RoleImportDetail> Details { get; set; } = new();
        public DateTime ImportDate { get; set; } = DateTime.UtcNow;
    }
}
