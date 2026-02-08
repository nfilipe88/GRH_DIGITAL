using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HRManager.WebAPI.Data
{
    public class HRManagerDbContextFactory : IDesignTimeDbContextFactory<HRManagerDbContext>
    {
        public HRManagerDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<HRManagerDbContext>();

            // Configure para usar PostgreSQL (ajuste a connection string conforme necessário)
            optionsBuilder.UseNpgsql("Host=localhost;Database=HRManager;Username=postgres;Password=your_password");

            return new HRManagerDbContext(optionsBuilder.Options);
        }
    }
}
