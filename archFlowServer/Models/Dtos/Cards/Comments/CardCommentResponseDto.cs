namespace archFlowServer.Models.Dtos.Cards.Comments;

public sealed record CardCommentResponseDto(
    int Id,
    int CardId,
    Guid UserId,
    string Content,
    int? ParentCommentId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);