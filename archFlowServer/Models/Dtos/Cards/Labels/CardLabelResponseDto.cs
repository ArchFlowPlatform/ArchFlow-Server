namespace archFlowServer.Models.Dtos.Cards.Labels;

public sealed record CardLabelResponseDto(
    int Id,
    int CardId,
    int LabelId,
    DateTime CreatedAt
);