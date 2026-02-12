using archFlowServer.Models.Exceptions;

namespace archFlowServer.Models.Entities;

public class CardAttachment
{
    public int Id { get; private set; }

    public int CardId { get; private set; }

    public string FileName { get; private set; } = string.Empty;   // original
    public string FilePath { get; private set; } = string.Empty;   // storage key/path/url
    public int? FileSize { get; private set; }
    public string MimeType { get; private set; } = string.Empty;

    public Guid UploadedBy { get; private set; }

    public DateTime CreatedAt { get; private set; }

    // navigations
    public BoardCard Card { get; private set; } = null!;

    private CardAttachment() { } // EF

    internal CardAttachment(
        int cardId,
        string fileName,
        string filePath,
        int? fileSize,
        string? mimeType,
        Guid uploadedBy)
    {
        if (cardId <= 0)
            throw new DomainException("CardId inválido.");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ValidationException("FileName é obrigatório.");

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ValidationException("FilePath é obrigatório.");

        if (uploadedBy == Guid.Empty)
            throw new DomainException("UploadedBy inválido.");

        if (fileName.Length > 255)
            throw new ValidationException("FileName não pode exceder 255 caracteres.");

        if (filePath.Length > 500)
            throw new ValidationException("FilePath não pode exceder 500 caracteres.");

        if (mimeType is not null && mimeType.Length > 100)
            throw new ValidationException("MimeType não pode exceder 100 caracteres.");

        CardId = cardId;
        FileName = fileName.Trim();
        FilePath = filePath.Trim();
        FileSize = fileSize;
        MimeType = (mimeType ?? string.Empty).Trim();

        UploadedBy = uploadedBy;

        CreatedAt = DateTime.UtcNow;
    }
}