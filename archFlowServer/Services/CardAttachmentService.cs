using archFlowServer.Models.Dtos.Cards.Attachments;
using archFlowServer.Models.Entities;
using archFlowServer.Models.Exceptions;
using archFlowServer.Repositories.Interfaces;

namespace archFlowServer.Services;

public class CardAttachmentService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICardAttachmentRepository _attachmentRepository;

    public CardAttachmentService(
        IProjectRepository projectRepository,
        ICardAttachmentRepository attachmentRepository)
    {
        _projectRepository = projectRepository;
        _attachmentRepository = attachmentRepository;
    }

    public async Task<IEnumerable<CardAttachmentResponseDto>> GetAllAsync(Guid projectId, int cardId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var list = await _attachmentRepository.GetAllAsync(projectId, cardId);
        return list.Select(MapToDto);
    }

    public async Task<CardAttachmentResponseDto> CreateAsync(Guid projectId, int cardId, CreateCardAttachmentDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var attachment = new CardAttachment(
            cardId: cardId,
            fileName: dto.FileName,
            filePath: dto.FilePath,
            fileSize: dto.FileSize,
            mimeType: dto.MimeType,
            uploadedBy: dto.UploadedBy
        );

        await _attachmentRepository.AddAsync(attachment);
        await _attachmentRepository.SaveChangesAsync();

        return MapToDto(attachment);
    }

    public async Task DeleteAsync(Guid projectId, int cardId, int attachmentId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var entity = await _attachmentRepository.GetByIdAsync(projectId, cardId, attachmentId)
            ?? throw new NotFoundException("Anexo não encontrado.");

        _attachmentRepository.Remove(entity);
        await _attachmentRepository.SaveChangesAsync();
    }

    private static CardAttachmentResponseDto MapToDto(CardAttachment a)
        => new(a.Id, a.CardId, a.FileName, a.FilePath, a.FileSize, a.MimeType, a.UploadedBy, a.CreatedAt);
}
