// Exemplo de código no HRManager.WebAPI/Data/HRManagerDbContext.cs

using HRManager.Application.Interfaces;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

public class HRManagerDbContext : DbContext
{
    private readonly ITenantService _tenantService;
    private readonly Guid? _currentTenantId;

    public HRManagerDbContext(DbContextOptions<HRManagerDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
        // Capturamos o ID no construtor para usar no filtro global
        _currentTenantId = _tenantService.GetTenantId();
    }

    // Mapeia os nossos modelos para tabelas no PostgreSQL
    public DbSet<Ausencia> Ausencias { get; set; }
    public DbSet<Avaliacao> Avaliacoes { get; set; }
    public DbSet<AvaliacaoItem> AvaliacaoItens { get; set; }
    public DbSet<CertificacaoProfissional> CertificacoesProfissionais { get; set; }
    public DbSet<CicloAvaliacao> CiclosAvaliacao { get; set; }
    public DbSet<Colaborador> Colaboradores { get; set; }
    public DbSet<Competencia> Competencias { get; set; }
    public DbSet<HabilitacaoLiteraria> HabilitacoesLiterarias { get; set; }
    public DbSet<Instituicao> Instituicoes { get; set; }
    public DbSet<Notificacao> Notificacoes { get; set; }
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

        // --- AQUI ESTÁ A MUDANÇA CRÍTICA ---
        // Aplicar Filtro Global para todas as entidades que implementam IHaveTenant
        // Se _currentTenantId for NULL (ex: Gestor Master ou Admin), o filtro não se aplica (mostra tudo).
        // Se tiver valor, filtra automaticamente.

        Expression<Func<IHaveTenant, bool>> tenantFilter = e =>
            _currentTenantId == null || e.InstituicaoId == _currentTenantId;

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IHaveTenant).IsAssignableFrom(entityType.ClrType)) // <--- AQUI
            {
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(ConvertFilterExpression(tenantFilter, entityType.ClrType));
            }
        }
    }

    // Método auxiliar para converter o filtro genérico para o tipo específico da entidade
    private static LambdaExpression ConvertFilterExpression(Expression<Func<IHaveTenant, bool>> filterExpression, Type entityType)
    {
        var newParam = Expression.Parameter(entityType);
        var newBody = ReplacingExpressionVisitor.Replace(filterExpression.Parameters.Single(), newParam, filterExpression.Body);
        return Expression.Lambda(newBody, newParam);
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