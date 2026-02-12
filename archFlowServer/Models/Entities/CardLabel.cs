using archFlowServer.Models.Exceptions;

namespace archFlowServer.Models.Entities;

public class CardLabel
{
    public int Id { get; private set; }

    public int CardId { get; private set; }
    public int LabelId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    // navigations
    public BoardCard Card { get; private set; } = null!;
    public Label Label { get; private set; } = null!;

    private CardLabel() { } // EF

    internal CardLabel(int cardId, int labelId)
    {
        if (cardId <= 0)
            throw new DomainException("CardId inválido.");

        if (labelId <= 0)
            throw new DomainException("LabelId inválido.");

        CardId = cardId;
        LabelId = labelId;

        CreatedAt = DateTime.UtcNow;
    }
}