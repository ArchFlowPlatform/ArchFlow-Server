using archFlowServer.Models.Dtos.Cards.Activities;
using archFlowServer.Models.Entities;
using archFlowServer.Models.Enums;
using archFlowServer.Models.Exceptions;
using archFlowServer.Repositories.Interfaces;

namespace archFlowServer.Services;

public class CardActivityService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICardActivityRepository _activityRepository;

    public CardActivityService(IProjectRepository projectRepository, ICardActivityRepository activityRepository)
    {
        _projectRepository = projectRepository;
        _activityRepository = activityRepository;
    }

    public async Task<IEnumerable<CardActivityResponseDto>> GetAllAsync(Guid projectId, int cardId, int take = 50)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        if (take <= 0 || take > 200)
            throw new ValidationException("Take deve estar entre 1 e 200.");

        var list = await _activityRepository.GetAllAsync(projectId, cardId, take);
        return list.Select(MapToDto);
    }

    // opcional: endpoint admin/debug para criar manualmente
    public async Task<CardActivityResponseDto> CreateAsync(Guid projectId, int cardId, CreateCardActivityDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var activity = new CardActivity(
            cardId: cardId,
            userId: dto.UserId,
            activityType: dto.ActivityType,
            oldValue: dto.OldValue,
            newValue: dto.NewValue,
            description: dto.Description
        );

        await _activityRepository.AddAsync(activity);
        await _activityRepository.SaveChangesAsync();

        return MapToDto(activity);
    }

    private static CardActivityResponseDto MapToDto(CardActivity a)
        => new(a.Id, a.CardId, a.UserId, a.ActivityType, a.OldValue, a.NewValue, a.Description, a.CreatedAt);
}
