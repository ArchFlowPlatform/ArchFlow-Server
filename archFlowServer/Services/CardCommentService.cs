using archFlowServer.Models.Dtos.Cards.Comments;
using archFlowServer.Models.Entities;
using archFlowServer.Models.Exceptions;
using archFlowServer.Repositories.Interfaces;

namespace archFlowServer.Services;

public class CardCommentService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICardCommentRepository _commentRepository;

    public CardCommentService(IProjectRepository projectRepository, ICardCommentRepository commentRepository)
    {
        _projectRepository = projectRepository;
        _commentRepository = commentRepository;
    }

    public async Task<IEnumerable<CardCommentResponseDto>> GetAllAsync(Guid projectId, int cardId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var list = await _commentRepository.GetAllAsync(projectId, cardId);
        return list.Select(MapToDto);
    }

    public async Task<CardCommentResponseDto> CreateAsync(Guid projectId, int cardId, CreateCardCommentDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        if (dto.ParentCommentId.HasValue)
        {
            var ok = await _commentRepository.ParentExistsOnSameCardAsync(cardId, dto.ParentCommentId.Value);
            if (!ok)
                throw new NotFoundException("ParentCommentId inválido para este card.");
        }

        var comment = new CardComment(cardId, dto.UserId, dto.Content, dto.ParentCommentId);

        await _commentRepository.AddAsync(comment);
        await _commentRepository.SaveChangesAsync();

        return MapToDto(comment);
    }

    public async Task<CardCommentResponseDto> UpdateAsync(Guid projectId, int cardId, int commentId, UpdateCardCommentDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var comment = await _commentRepository.GetByIdAsync(projectId, cardId, commentId)
            ?? throw new NotFoundException("Comentário não encontrado.");

        comment.UpdateContent(dto.Content);

        await _commentRepository.SaveChangesAsync();

        return MapToDto(comment);
    }

    public async Task DeleteAsync(Guid projectId, int cardId, int commentId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var comment = await _commentRepository.GetByIdAsync(projectId, cardId, commentId)
            ?? throw new NotFoundException("Comentário não encontrado.");

        _commentRepository.Remove(comment);
        await _commentRepository.SaveChangesAsync();
    }

    private static CardCommentResponseDto MapToDto(CardComment c)
        => new(c.Id, c.CardId, c.UserId, c.Content, c.ParentCommentId, c.CreatedAt, c.UpdatedAt);
}
