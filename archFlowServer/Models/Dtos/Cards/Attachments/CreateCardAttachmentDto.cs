namespace archFlowServer.Models.Dtos.Cards.Attachments;

public class CreateCardAttachmentDto
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;

    public int? FileSize { get; set; }
    public string? MimeType { get; set; }
    public Guid UploadedBy { get; set; } // vindo do token em produção, aqui no dto por enquanto
}