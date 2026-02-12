namespace archFlowServer.Models.Dtos.Cards.Attachments;

public sealed record CardAttachmentResponseDto(
    int Id,
    int CardId,
    string FileName,
    string FilePath,
    int? FileSize,
    string MimeType,
    Guid UploadedBy,
    DateTime CreatedAt
);
