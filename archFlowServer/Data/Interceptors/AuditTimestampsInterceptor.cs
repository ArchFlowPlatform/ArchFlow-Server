using archFlowServer.Models.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace archFlowServer.Data.Interceptors;

public sealed class AuditTimestampsInterceptor : SaveChangesInterceptor
{
    private static DateTime UtcNow() => DateTime.UtcNow;

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ApplyAudit(DbContext? context)
    {
        if (context is null) return;

        var now = UtcNow();

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                // CreatedAt e UpdatedAt no insert
                entry.Entity.SetCreatedAt(now);
                entry.Entity.SetUpdatedAt(now);
            }
            else if (entry.State == EntityState.Modified)
            {
                // impede mudar CreatedAt "sem querer"
                entry.Property(nameof(IAuditableEntity.CreatedAt)).IsModified = false;

                entry.Entity.SetUpdatedAt(now);
            }
        }
    }
}