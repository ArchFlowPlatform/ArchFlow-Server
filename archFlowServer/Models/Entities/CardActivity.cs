using archFlowServer.Models.Enums;
using archFlowServer.Models.Exceptions;

namespace archFlowServer.Models.Entities;

public class CardActivity
{
    public int Id { get; private set; }

    public int CardId { get; private set; }
    public Guid UserId { get; private set; }

    public CardActivityType ActivityType { get; private set; }

    // jsonb
    public string OldValue { get; private set; } = string.Empty;
    public string NewValue { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    // navigations
    public BoardCard Card { get; private set; } = null!;

    private CardActivity() { } // EF

    internal CardActivity(
        int cardId,
        Guid userId,
        CardActivityType activityType,
        string? oldValue,
        string? newValue,
        string? description)
    {
        if (cardId <= 0)
            throw new DomainException("CardId inválido.");

        if (userId == Guid.Empty)
            throw new DomainException("UserId inválido.");

        CardId = cardId;
        UserId = userId;
        ActivityType = activityType;

        OldValue = (oldValue ?? string.Empty).Trim();
        NewValue = (newValue ?? string.Empty).Trim();
        Description = (description ?? string.Empty).Trim();

        CreatedAt = DateTime.UtcNow;
    }
}