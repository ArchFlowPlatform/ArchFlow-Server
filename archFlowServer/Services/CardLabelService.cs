using archFlowServer.Models.Dtos.Cards.Labels;
using archFlowServer.Models.Entities;
using archFlowServer.Models.Exceptions;
using archFlowServer.Repositories.Interfaces;

namespace archFlowServer.Services;

public class CardLabelService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILabelRepository _labelRepository;
    private readonly ICardLabelRepository _cardLabelRepository;

    public CardLabelService(
        IProjectRepository projectRepository,
        ILabelRepository labelRepository,
        ICardLabelRepository cardLabelRepository)
    {
        _projectRepository = projectRepository;
        _labelRepository = labelRepository;
        _cardLabelRepository = cardLabelRepository;
    }

    public async Task<IEnumerable<CardLabelResponseDto>> GetAllAsync(Guid projectId, int cardId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var list = await _cardLabelRepository.GetAllAsync(projectId, cardId);
        return list.Select(MapToDto);
    }

    public async Task<CardLabelResponseDto> AddAsync(Guid projectId, int cardId, AddCardLabelDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        if (dto.LabelId <= 0)
            throw new ValidationException("LabelId inválido.");

        // label precisa ser do projeto
        _ = await _labelRepository.GetByIdAsync(projectId, dto.LabelId)
            ?? throw new NotFoundException("Label não encontrada.");

        if (await _cardLabelRepository.ExistsAsync(cardId, dto.LabelId))
            throw new ConflictException("Esta label já está vinculada ao card.");

        var entity = new CardLabel(cardId, dto.LabelId);

        await _cardLabelRepository.AddAsync(entity);
        await _cardLabelRepository.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task RemoveAsync(Guid projectId, int cardId, int cardLabelId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var entity = await _cardLabelRepository.GetByIdAsync(projectId, cardId, cardLabelId)
            ?? throw new NotFoundException("Vínculo label-card não encontrado.");

        _cardLabelRepository.Remove(entity);
        await _cardLabelRepository.SaveChangesAsync();
    }

    private static CardLabelResponseDto MapToDto(CardLabel x)
        => new(x.Id, x.CardId, x.LabelId, x.CreatedAt);
}
