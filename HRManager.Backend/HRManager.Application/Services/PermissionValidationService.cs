using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.Helpers;
using Microsoft.EntityFrameworkCore;

namespace HRManager.WebAPI.Services
{
    public class PermissionValidationService : IPermissionValidationService
    {
        private readonly HRManagerDbContext _context;
        private readonly ILogger<PermissionValidationService> _logger;

        public PermissionValidationService(
            HRManagerDbContext context,
            ILogger<PermissionValidationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ValidationResult> ValidateRolePermissionsAsync(Guid roleId, List<Guid> permissionIds)
        {
            var result = new ValidationResult();

            var permissionCodes = await _context.Permissions
                .Where(p => permissionIds.Contains(p.Id))
                .Select(p => p.Code)
                .ToListAsync();

            result.Dependencies = await CheckDependenciesAsync(permissionCodes);
            result.Conflicts = await GetPermissionConflictsAsync(permissionCodes);
            result.BusinessRuleViolations = await CheckBusinessRulesAsync(permissionCodes);

            result.IsValid = !result.HasErrors;
            result.Severity = result.GetOverallSeverity();

            return result;
        }

        public async Task<ValidationResult> ValidateUserPermissionsAsync(Guid userId, List<Guid> roleIds)
        {
            var result = new ValidationResult();
            // Implementação simplificada - pode ser expandida
            return await Task.FromResult(result);
        }

        public async Task<List<PermissionDependency>> GetPermissionDependenciesAsync(string permissionCode)
        {
            var dependencies = new Dictionary<string, List<string>>
            {
                { "USERS_DELETE", new List<string> { "USERS_VIEW", "USERS_EDIT" } },
                { "USERS_EDIT", new List<string> { "USERS_VIEW" } },
                { "USERS_CHANGE_ROLE", new List<string> { "USERS_VIEW", "ROLES_VIEW" } },
                { "ROLES_DELETE", new List<string> { "ROLES_VIEW" } },
                { "ROLES_EDIT", new List<string> { "ROLES_VIEW" } },
                { "ROLES_MANAGE_PERMISSIONS", new List<string> { "ROLES_VIEW", "PERMISSIONS_VIEW" } },
                { "INSTITUTIONS_DELETE", new List<string> { "INSTITUTIONS_VIEW" } },
                { "INSTITUTIONS_EDIT", new List<string> { "INSTITUTIONS_VIEW" } },
                { "ABSENCES_APPROVE", new List<string> { "ABSENCES_VIEW_ALL" } },
                { "ABSENCES_MANAGE", new List<string> { "ABSENCES_VIEW_ALL", "ABSENCES_APPROVE" } },
                { "REPORTS_GENERATE", new List<string> { "REPORTS_VIEW" } },
                { "REPORTS_EXPORT", new List<string> { "REPORTS_VIEW" } }
            };

            if (!dependencies.ContainsKey(permissionCode))
                return new List<PermissionDependency>();

            var requiredCodes = dependencies[permissionCode];
            var requiredPermissions = await _context.Permissions
                .Where(p => requiredCodes.Contains(p.Code))
                .Select(p => new PermissionDependency
                {
                    PermissionCode = p.Code,
                    PermissionName = p.Name,
                    DependencyType = "REQUIRED",
                    Description = $"Necessário para {permissionCode}",
                    Severity = "ERROR"
                })
                .ToListAsync();

            return requiredPermissions;
        }

        public async Task<List<PermissionConflict>> GetPermissionConflictsAsync(List<string> permissionCodes)
        {
            var conflicts = new List<PermissionConflict>();

            var conflictRules = new List<ConflictRule>
            {
                new ConflictRule
                {
                    PermissionA = "USERS_DELETE",
                    PermissionB = "USERS_CREATE",
                    ConflictType = "MUTUALLY_EXCLUSIVE",
                    Severity = "ERROR",
                    Description = "Não pode ter permissão para deletar e criar usuários simultaneamente"
                },
                new ConflictRule
                {
                    PermissionA = "INSTITUTIONS_DELETE",
                    PermissionB = "INSTITUTIONS_CREATE",
                    ConflictType = "MUTUALLY_EXCLUSIVE",
                    Severity = "ERROR",
                    Description = "Não pode gerenciar instituições de forma completa"
                }
            };

            foreach (var rule in conflictRules)
            {
                if (permissionCodes.Contains(rule.PermissionA) && permissionCodes.Contains(rule.PermissionB))
                {
                    conflicts.Add(new PermissionConflict
                    {
                        PermissionCodeA = rule.PermissionA,
                        PermissionCodeB = rule.PermissionB,
                        ConflictType = rule.ConflictType,
                        Severity = rule.Severity,
                        Description = rule.Description,
                        Resolution = "Remova uma das permissões conflitantes"
                    });
                }
            }

            return await Task.FromResult(conflicts);
        }

        public async Task<PermissionCompatibilityReport> CheckCompatibilityAsync(List<string> permissionCodes)
        {
            var report = new PermissionCompatibilityReport
            {
                PermissionCodes = permissionCodes,
                CheckedAt = DateTime.UtcNow
            };

            var allDependencies = new List<PermissionDependency>();
            foreach (var code in permissionCodes)
            {
                var dependencies = await GetPermissionDependenciesAsync(code);
                allDependencies.AddRange(dependencies);
            }

            var missingDependencies = allDependencies
                .Where(d => !permissionCodes.Contains(d.PermissionCode))
                .ToList();

            report.MissingDependencies = missingDependencies;
            report.Conflicts = await GetPermissionConflictsAsync(permissionCodes);

            report.Metrics = new CompatibilityMetrics
            {
                TotalPermissions = permissionCodes.Count,
                MissingDependenciesCount = missingDependencies.Count,
                ConflictCount = report.Conflicts.Count,
                ErrorCount = report.Conflicts.Count(c => c.Severity == "ERROR"),
                WarningCount = report.Conflicts.Count(c => c.Severity == "WARNING"),
                CompatibilityScore = CalculateCompatibilityScore(permissionCodes, missingDependencies, report.Conflicts)
            };

            report.IsCompatible = report.Metrics.ErrorCount == 0 && report.Metrics.MissingDependenciesCount == 0;
            report.Severity = report.Metrics.ErrorCount > 0 ? "ERROR" :
                             report.Metrics.WarningCount > 0 ? "WARNING" : "INFO";

            return report;
        }

        private async Task<List<PermissionDependency>> CheckDependenciesAsync(List<string> permissionCodes)
        {
            var results = new List<PermissionDependency>();

            foreach (var code in permissionCodes)
            {
                var dependencies = await GetPermissionDependenciesAsync(code);
                var missing = dependencies
                    .Where(d => !permissionCodes.Contains(d.PermissionCode))
                    .ToList();

                results.AddRange(missing);
            }

            return results;
        }

        private async Task<List<BusinessRuleViolation>> CheckBusinessRulesAsync(List<string> permissionCodes)
        {
            var violations = new List<BusinessRuleViolation>();

            var deletePermissions = permissionCodes.Where(c => c.EndsWith("_DELETE")).ToList();
            foreach (var deletePerm in deletePermissions)
            {
                var viewPerm = deletePerm.Replace("_DELETE", "_VIEW");
                if (!permissionCodes.Contains(viewPerm))
                {
                    violations.Add(new BusinessRuleViolation
                    {
                        RuleCode = "BR001",
                        RuleName = "Delete Requires View",
                        Description = $"Permissão '{deletePerm}' requer permissão '{viewPerm}'",
                        Severity = "ERROR",
                        AffectedPermissions = new List<string> { deletePerm, viewPerm }
                    });
                }
            }

            return await Task.FromResult(violations);
        }

        private double CalculateCompatibilityScore(
            List<string> permissionCodes,
            List<PermissionDependency> missingDependencies,
            List<PermissionConflict> conflicts)
        {
            if (!permissionCodes.Any())
                return 0.0;

            double score = 100.0;
            score -= missingDependencies.Count * 10.0;
            score -= conflicts.Count(c => c.Severity == "ERROR") * 20.0;
            score -= conflicts.Count(c => c.Severity == "WARNING") * 5.0;

            return Math.Max(0.0, score);
        }
    }
}
