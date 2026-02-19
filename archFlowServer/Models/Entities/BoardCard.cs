using archFlowServer.Models.Exceptions;

namespace archFlowServer.Models.Entities;

public class BoardCard
{
    public int Id { get; private set; }

    public int ColumnId { get; private set; }

    // ✅ Padrão A: card sempre representa UserStory
    public int UserStoryId { get; private set; }

    public int Position { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // navigations
    public BoardColumn Column { get; private set; } = null!;
    public UserStory UserStory { get; private set; } = null!;

    private BoardCard() { } // EF

    internal BoardCard(int columnId, int userStoryId, int position)
    {
        if (columnId <= 0) throw new DomainException("ColumnId inválido.");
        if (userStoryId <= 0) throw new DomainException("UserStoryId inválido.");
        if (position < 0) throw new ValidationException("Position inválida.");

        ColumnId = columnId;
        UserStoryId = userStoryId;
        Position = position;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MoveToColumn(int columnId)
    {
        if (columnId <= 0) throw new DomainException("ColumnId inválido.");
        ColumnId = columnId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPosition(int position)
    {
        if (position < 0) throw new ValidationException("Position inválida.");
        Position = position;
        UpdatedAt = DateTime.UtcNow;
    }
}
