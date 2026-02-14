using archFlowServer.Models.Enums;

namespace archFlowServer.Models.Dtos.Cards.Activities;

public sealed record CardActivityResponseDto(
    int Id,
    int CardId,
    Guid UserId,
    CardActivityType ActivityType,
    string OldValue,
    string NewValue,
    string Description,
    DateTime CreatedAt
);