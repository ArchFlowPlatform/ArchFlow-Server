namespace archFlowServer.Models.Dtos.Cards.Comments;

public class CreateCardCommentDto
{
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;

    public int? ParentCommentId { get; set; }
}