using HRManager.Application.Interfaces;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.Models;
using System.Text.Json;
using HRManager.WebAPI.Domain.enums;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// Classe auxiliar temporária (podes colocar dentro do DbContext ou num ficheiro separado)
public class AuditEntryHelper
{
    public AuditLog AuditLog { get; set; } = null!;
    public EntityEntry Entry { get; set; } = null!;
}

public class HRManagerDbContext : IdentityDbContext<User, Role, Guid, IdentityUserClaim<Guid>,
        UserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
{
    private readonly ITenantService? _tenantService;

    public HRManagerDbContext(DbContextOptions<HRManagerDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    public HRManagerDbContext(DbContextOptions<HRManagerDbContext> options)
        : base(options)
    {
        _tenantService = null;
    }

    // DbSets (Mantive a lista resumida, assume-se que estão todos aqui como no teu ficheiro original)
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Ausencia> Ausencias { get; set; }
    public DbSet<Avaliacao> Avaliacoes { get; set; }
    public DbSet<AvaliacaoItem> AvaliacaoItens { get; set; }
    public DbSet<Cargo> Cargos { get; set; }
    public DbSet<CertificacaoProfissional> CertificacoesProfissionais { get; set; }
    public DbSet<CicloAvaliacao> CiclosAvaliacao { get; set; }
    public DbSet<Colaborador> Colaboradores { get; set; }
    public DbSet<Competencia> Competencias { get; set; }
    public DbSet<HabilitacaoLiteraria> HabilitacoesLiterarias { get; set; }
    public DbSet<Instituicao> Instituicoes { get; set; }
    public DbSet<Notificacao> Notificacoes { get; set; }
    public DbSet<PedidoDeclaracao> PedidosDeclaracao { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<RoleTemplate> RoleTemplates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Configuração de UserRole (N:N)
        modelBuilder.Entity<UserRole>(b =>
        {
            b.HasKey(ur => new { ur.UserId, ur.RoleId });
            b.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            b.ToTable("UserRoles");
        });

        // Configuração de nomes das tabelas
        modelBuilder.Entity<User>(b => b.ToTable("Users"));
        modelBuilder.Entity<Role>(b => b.ToTable("Roles"));
        modelBuilder.Entity<IdentityUserClaim<Guid>>(b => b.ToTable("UserClaims"));
        modelBuilder.Entity<IdentityUserLogin<Guid>>(b => b.ToTable("UserLogins"));
        modelBuilder.Entity<IdentityRoleClaim<Guid>>(b => b.ToTable("RoleClaims"));
        modelBuilder.Entity<IdentityUserToken<Guid>>(b => b.ToTable("UserTokens"));

        // RolePermission e Índices
        modelBuilder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });
        modelBuilder.Entity<Permission>().HasIndex(p => p.Code).IsUnique();
        modelBuilder.Entity<Role>().HasIndex(r => new { r.Name, r.InstituicaoId }).IsUnique();

        // Configurações Decimal e Chaves Estrangeiras
        modelBuilder.Entity<Colaborador>().Property(c => c.SalarioBase).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Colaborador>().HasOne(c => c.Gestor).WithMany(g => g.Subordinados).HasForeignKey(c => c.GestorId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Avaliacao>().Property(a => a.MediaFinal).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Cargo>().HasIndex(c => new { c.Nome, c.InstituicaoId }).IsUnique();

        // Filtro Global Multi-Tenant
        if (_tenantService != null)
        {
            var tenantId = _tenantService.GetTenantId();
            if (tenantId != Guid.Empty)
            {
                foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                {
                    if (typeof(IHaveTenant).IsAssignableFrom(entityType.ClrType))
                    {
                        var method = typeof(HRManagerDbContext)
                            .GetMethod(nameof(SetGlobalQueryFilter),
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                            ?.MakeGenericMethod(entityType.ClrType);
                        method?.Invoke(null, new object[] { modelBuilder, tenantId });
                    }
                }
            }
        }
    }

    private static void SetGlobalQueryFilter<T>(ModelBuilder builder, Guid tenantId) where T : class, IHaveTenant
    {
        builder.Entity<T>().HasQueryFilter(e => e.InstituicaoId == tenantId);
    }

    // --- CORREÇÃO AQUI: SaveChanges Síncrono também deve usar a lógica unificada ---
    public override int SaveChanges()
    {
        // Se quiseres auditoria no Sync também, chama o OnBefore/OnAfter.
        // Para simplificar e focar na correção do Tenant, vamos garantir que a lógica corre.
        var auditHelpers = OnBeforeSaveChanges();
        var result = base.SaveChanges();
        if (auditHelpers.Count > 0) OnAfterSaveChangesSync(auditHelpers); // Precisarias de uma versão Sync do After
        return result;
    }

    // O ASYNC é o mais importante para a tua WebAPI
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Detetar alterações, atribuir Tenants e preparar logs
        var auditEntryHelpers = OnBeforeSaveChanges();

        // 2. Salvar na BD (Gera IDs)
        var result = await base.SaveChangesAsync(cancellationToken);

        // 3. Atualizar IDs nos logs e salvar logs
        if (auditEntryHelpers.Count > 0)
        {
            await OnAfterSaveChanges(auditEntryHelpers);
        }

        return result;
    }

    private List<AuditEntryHelper> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditHelpers = new List<AuditEntryHelper>();
        var userId = Guid.Empty; // Podes obter via TokenService se quiseres ou manter Empty

        // CORREÇÃO: Obter TenantId aqui para aplicar a todas as entidades
        var currentTenantId = _tenantService?.GetInstituicaoId() ?? Guid.Empty;

        foreach (var entry in ChangeTracker.Entries())
        {
            // --- 1. ATRIBUIÇÃO AUTOMÁTICA DE TENANT (Movido para aqui) ---
            if (entry.State == EntityState.Added && entry.Entity is IHaveTenant tenantEntity)
            {
                // Só preenche se estiver vazio e tivermos um tenant válido no contexto
                if (tenantEntity.InstituicaoId == Guid.Empty && currentTenantId != Guid.Empty)
                {
                    tenantEntity.InstituicaoId = currentTenantId;
                }
            }
            // -------------------------------------------------------------

            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var action = entry.State switch
            {
                EntityState.Added => AuditAction.Created,
                EntityState.Modified => AuditAction.Updated,
                EntityState.Deleted => AuditAction.Deleted,
                _ => AuditAction.None
            };

            // Agora o TenantId da entidade já estará preenchido corretamente
            var entityTenantId = (entry.Entity as IHaveTenant)?.InstituicaoId ?? Guid.Empty;

            var auditEntry = new AuditLog
            {
                Action = action,
                EntityName = entry.Entity.GetType().Name,
                DataCriacao = DateTime.UtcNow,
                UserId = userId,
                EntityId = Guid.Empty, // Será preenchido no OnAfterSaveChanges para Created
                InstituicaoId = entityTenantId
            };

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary) continue;
                string propertyName = property.Metadata.Name;

                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues[propertyName] = property.CurrentValue;
                        break;
                    case EntityState.Deleted:
                        oldValues[propertyName] = property.OriginalValue;
                        if (property.Metadata.IsPrimaryKey()) auditEntry.EntityId = (Guid)property.OriginalValue!;
                        break;
                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            oldValues[propertyName] = property.OriginalValue;
                            newValues[propertyName] = property.CurrentValue;
                        }
                        if (property.Metadata.IsPrimaryKey()) auditEntry.EntityId = (Guid)property.CurrentValue!;
                        break;
                }
            }

            auditEntry.OldValues = oldValues.Count == 0 ? null : JsonSerializer.Serialize(oldValues);
            auditEntry.NewValues = newValues.Count == 0 ? null : JsonSerializer.Serialize(newValues);

            auditHelpers.Add(new AuditEntryHelper { AuditLog = auditEntry, Entry = entry });
        }

        return auditHelpers;
    }

    private async Task OnAfterSaveChanges(List<AuditEntryHelper> auditHelpers)
    {
        if (auditHelpers == null || auditHelpers.Count == 0) return;

        foreach (var helper in auditHelpers)
        {
            if (helper.AuditLog.Action == AuditAction.Created)
            {
                var primaryKey = helper.Entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                if (primaryKey != null && primaryKey.CurrentValue != null)
                {
                    if (Guid.TryParse(primaryKey.CurrentValue.ToString(), out Guid id))
                    {
                        helper.AuditLog.EntityId = id;
                    }
                }
            }
            AuditLogs.Add(helper.AuditLog);
        }
        await base.SaveChangesAsync();
    }

    // Versão Síncrona simples (opcional, só se usares SaveChanges sync)
    private void OnAfterSaveChangesSync(List<AuditEntryHelper> auditHelpers)
    {
        foreach (var helper in auditHelpers)
        {
            if (helper.AuditLog.Action == AuditAction.Created)
            {
                var primaryKey = helper.Entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                if (primaryKey != null && primaryKey.CurrentValue != null && Guid.TryParse(primaryKey.CurrentValue.ToString(), out Guid id))
                    helper.AuditLog.EntityId = id;
            }
            AuditLogs.Add(helper.AuditLog);
        }
        base.SaveChanges();
    }
}