using archFlowServer.Models.Enums;

namespace archFlowServer.Models.Dtos.Cards.Activities;

public class CreateCardActivityDto
{
    public Guid UserId { get; set; }
    public CardActivityType ActivityType { get; set; }

    // JSON string (persistido como jsonb)
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public string? Description { get; set; }
}