// Exemplo de código no HRManager.WebAPI/Data/HRManagerDbContext.cs

using HRManager.Application.Interfaces;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

public class HRManagerDbContext : DbContext
{
    private readonly ITenantService _tenantService;
    
    // Injetar o TenantService
    public HRManagerDbContext(DbContextOptions<HRManagerDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    // Mapeia os nossos modelos para tabelas no PostgreSQL
    public DbSet<Ausencia> Ausencias { get; set; }
    public DbSet<CertificacaoProfissional> CertificacoesProfissionais { get; set; }
    public DbSet<Colaborador> Colaboradores { get; set; }
    public DbSet<HabilitacaoLiteraria> HabilitacoesLiterarias { get; set; }
    public DbSet<Instituicao> Instituicoes { get; set; }
    public DbSet<PedidoDeclaracao> PedidosDeclaracao { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //[cite_start]// Configurar regras do modelo (baseado na RN-01.1) [cite: 73]
        modelBuilder.Entity<Instituicao>()
            .HasIndex(i => i.IdentificadorUnico)
            .IsUnique();

        //[cite_start]// Configurar regras do modelo (baseado na RN-02.1) [cite: 88]
        modelBuilder.Entity<Colaborador>()
            .HasIndex(c => new { c.NIF, c.InstituicaoId }) // NIF deve ser único POR instituição
            .IsUnique();
        // Garante que não existem dois utilizadores com o mesmo email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }

    public override int SaveChanges()
    {
        var instituicaoId = _tenantService.GetInstituicaoId();

        foreach (var entry in ChangeTracker.Entries<IHaveTenant>())
        {
            // Apenas para entidades novas (Added)
            if (entry.State == EntityState.Added)
            {
                // Atribui o ID da Instituição ao novo registo
                entry.Entity.InstituicaoId = instituicaoId;
            }
        }
        return base.SaveChanges();
    }
}