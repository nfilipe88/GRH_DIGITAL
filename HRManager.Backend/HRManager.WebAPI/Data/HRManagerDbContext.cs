using HRManager.Application.Interfaces;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

public class HRManagerDbContext : IdentityDbContext<User, Role, Guid, IdentityUserClaim<Guid>,
        UserRole,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>>
{
    private readonly ITenantService _tenantService;

    public HRManagerDbContext(DbContextOptions<HRManagerDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    public DbSet<Avaliacao> Avaliacoes { get; set; }
    public DbSet<AvaliacaoItem> AvaliacaoItens { get; set; }
    public DbSet<Ausencia> Ausencias { get; set; }
    public DbSet<Cargo> Cargos { get; set; }
    public DbSet<CertificacaoProfissional> CertificacoesProfissionais { get; set; }
    public DbSet<CicloAvaliacao> CiclosAvaliacao { get; set; }
    public DbSet<Colaborador> Colaboradores { get; set; }
    public DbSet<Competencia> Competencias { get; set; }
    public DbSet<HabilitacaoLiteraria> HabilitacoesLiterarias { get; set; }
    public DbSet<Instituicao> Instituicoes { get; set; }
    public DbSet<Notificacao> Notificacoes { get; set; }
    public DbSet<PedidoDeclaracao> PedidosDeclaracao { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Configuração de UserRole (N:N)
        // ALTERAÇÃO 2: Configuração correta da tabela UserRoles personalizada
        modelBuilder.Entity<UserRole>(b =>
        {
            b.HasKey(ur => new { ur.UserId, ur.RoleId }); // Define a chave composta

            b.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

            b.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            b.ToTable("UserRoles"); // Define o nome da tabela
        });
        // Configuração de nomes das tabelas (Opcional, mas recomendado para ficar limpo no PostgreSQL)
        modelBuilder.Entity<User>(b => b.ToTable("Users"));
        modelBuilder.Entity<Role>(b => b.ToTable("Roles"));
        modelBuilder.Entity<IdentityUserClaim<Guid>>(b => b.ToTable("UserClaims"));
        modelBuilder.Entity<IdentityUserLogin<Guid>>(b => b.ToTable("UserLogins"));
        modelBuilder.Entity<IdentityRoleClaim<Guid>>(b => b.ToTable("RoleClaims"));
        modelBuilder.Entity<IdentityUserToken<Guid>>(b => b.ToTable("UserTokens"));

        // 2. Configurações Específicas
        modelBuilder.Entity<Colaborador>()
            .Property(c => c.SalarioBase)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Colaborador>()
            .HasOne(c => c.Gestor)
            .WithMany(g => g.Subordinados)
            .HasForeignKey(c => c.GestorId)
            .OnDelete(DeleteBehavior.Restrict); // IMPEDE que apagar o Chefe apague a equipa

        modelBuilder.Entity<Avaliacao>()
            .Property(a => a.MediaFinal)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Cargo>()
            .HasIndex(c => new { c.Nome, c.InstituicaoId })
            .IsUnique();

        // NOTA: REMOVEMOS O HasData(Roles) DAQUI PARA O DBSEEDER
        // Para evitar o erro de modelo dinâmico com datas.

        // 3. Filtro Global Multi-Tenant
        // Aplica-se a todas as entidades que implementam IHaveTenant
        // A Instituicao NÃO implementa IHaveTenant propositadamente (ela é a raiz)
        var tenantId = _tenantService.GetTenantId();

        if (tenantId != Guid.Empty && tenantId != null)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(IHaveTenant).IsAssignableFrom(entityType.ClrType))
                {
                    // Configura o filtro global
                    var method = typeof(HRManagerDbContext)
                        .GetMethod(nameof(SetGlobalQueryFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                        ?.MakeGenericMethod(entityType.ClrType);

                    method?.Invoke(null, new object[] { modelBuilder, tenantId });
                }
            }
        }
    }

    private static void SetGlobalQueryFilter<T>(ModelBuilder builder, Guid tenantId) where T : class, IHaveTenant
    {
        builder.Entity<T>().HasQueryFilter(e => e.InstituicaoId == tenantId);
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
        // Obtém o ID do tenant atual (pode ser Empty se for via Seeder/Console)
        var instituicaoId = _tenantService.GetInstituicaoId();

        foreach (var entry in ChangeTracker.Entries<IHaveTenant>())
        {
            // Apenas para entidades novas (Added)
            if (entry.State == EntityState.Added)
            {
                // CORREÇÃO:
                // Apenas sobrescreve se o ID atual for Empty.
                // Se o Seeder já definiu um ID, mantemos esse ID.
                if (entry.Entity.InstituicaoId == Guid.Empty && instituicaoId != Guid.Empty)
                {
                    entry.Entity.InstituicaoId = instituicaoId;
                }
                // Opcional: Se quiseres forçar que, se não houver ID manual NEM tenant logado,
                // e for uma operação normal, deve falhar ou assumir algo, mas a lógica acima
                // é a mais segura para Seeders.
            }
        }
        return base.SaveChanges();
    }
}