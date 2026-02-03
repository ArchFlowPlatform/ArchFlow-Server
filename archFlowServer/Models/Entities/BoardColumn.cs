using archFlowServer.Models.Exceptions;

namespace archFlowServer.Models.Entities;

public class BoardColumn
{
    private readonly List<BoardCard> _cards = new();

    public int Id { get; private set; }

    public Guid BoardId { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public int Position { get; private set; }
    public int? WipLimit { get; private set; }

    public string Color { get; private set; } = "#95a5a6";
    public bool IsDoneColumn { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // navigations
    public Board Board { get; private set; } = null!;
    public IReadOnlyCollection<BoardCard> Cards => _cards.AsReadOnly();

    private BoardColumn() { } // EF

    internal BoardColumn(
        Guid boardId,
        string name,
        string? description,
        int position,
        int? wipLimit,
        string? color,
        bool isDoneColumn)
    {
        if (boardId == Guid.Empty)
            throw new DomainException("BoardId inválido.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Nome da coluna é obrigatório.");

        if (position < 0)
            throw new ValidationException("Position inválida.");

        if (wipLimit is < 0)
            throw new ValidationException("WipLimit não pode ser negativo.");

        BoardId = boardId;
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;

        Position = position;
        WipLimit = wipLimit;

        Color = NormalizeHex(color);
        IsDoneColumn = isDoneColumn;
    }

    public void Update(
        string name,
        string? description,
        int? wipLimit,
        string? color,
        bool isDoneColumn)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Nome da coluna é obrigatório.");

        if (wipLimit is < 0)
            throw new ValidationException("WipLimit não pode ser negativo.");

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;

        WipLimit = wipLimit;
        Color = NormalizeHex(color);
        IsDoneColumn = isDoneColumn;
    }

    public void SetPosition(int position)
    {
        if (position < 0)
            throw new ValidationException("Position inválida.");

        Position = position;
    }

    private static string NormalizeHex(string? color)
    {
        var c = (color ?? "#95a5a6").Trim();
        if (c.Length != 7 || c[0] != '#')
            throw new ValidationException("Color inválida. Use formato #RRGGBB.");

        return c.ToLowerInvariant();
    }
}
