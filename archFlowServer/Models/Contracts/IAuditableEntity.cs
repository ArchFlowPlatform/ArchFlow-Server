namespace archFlowServer.Models.Contracts;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; }
    DateTime UpdatedAt { get; }

    void SetCreatedAt(DateTime utcNow);
    void SetUpdatedAt(DateTime utcNow);
}