using archFlowServer.Models.Exceptions;

namespace archFlowServer.Models.Entities;

public class CardComment
{
    public int Id { get; private set; }

    public int CardId { get; private set; }
    public Guid UserId { get; private set; }

    public string Content { get; private set; } = string.Empty;

    public int? ParentCommentId { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // navigations
    public BoardCard Card { get; private set; } = null!;
    public CardComment? ParentComment { get; private set; }
    private readonly List<CardComment> _replies = new();
    public IReadOnlyCollection<CardComment> Replies => _replies.AsReadOnly();

    private CardComment() { } // EF

    internal CardComment(int cardId, Guid userId, string content, int? parentCommentId)
    {
        if (cardId <= 0)
            throw new DomainException("CardId inválido.");

        if (userId == Guid.Empty)
            throw new DomainException("UserId inválido.");

        if (string.IsNullOrWhiteSpace(content))
            throw new ValidationException("Conteúdo do comentário é obrigatório.");

        CardId = cardId;
        UserId = userId;

        Content = content.Trim();
        ParentCommentId = parentCommentId;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ValidationException("Conteúdo do comentário é obrigatório.");

        Content = content.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}