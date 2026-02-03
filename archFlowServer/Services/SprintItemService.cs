using archFlowServer.Models.Dtos.Sprint;
using archFlowServer.Models.Entities;
using archFlowServer.Models.Enums;
using archFlowServer.Models.Exceptions;
using archFlowServer.Repositories.Interfaces;

namespace archFlowServer.Services;

public class SprintItemService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ISprintRepository _sprintRepository;
    private readonly IUserStoryRepository _userStoryRepository;
    private readonly ISprintItemRepository _sprintItemRepository;

    public SprintItemService(
        IProjectRepository projectRepository,
        ISprintRepository sprintRepository,
        IUserStoryRepository userStoryRepository,
        ISprintItemRepository sprintItemRepository)
    {
        _projectRepository = projectRepository;
        _sprintRepository = sprintRepository;
        _userStoryRepository = userStoryRepository;
        _sprintItemRepository = sprintItemRepository;
    }

    // ==========================
    // Read
    // ==========================

    public async Task<IEnumerable<SprintItemResponseDto>> GetAllAsync(Guid projectId, Guid sprintId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        _ = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        var items = await _sprintItemRepository.GetAllBySprintAsync(projectId, sprintId);
        return items.Select(MapToDto);
    }

    public async Task<SprintItemResponseDto> GetByIdAsync(Guid projectId, Guid sprintId, int sprintItemId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        _ = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        var item = await _sprintItemRepository.GetByIdAsync(projectId, sprintId, sprintItemId)
            ?? throw new NotFoundException("Item do sprint não encontrado.");

        return MapToDto(item);
    }

    // ==========================
    // Create
    // ==========================

    public async Task<SprintItemResponseDto> CreateAsync(Guid projectId, Guid sprintId, CreateSprintItemDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var sprint = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        if (sprint.Status is SprintStatus.Closed or SprintStatus.Cancelled)
            throw new ConflictException("Não é possível alterar uma sprint fechada/cancelada.");

        if (dto.UserStoryId <= 0)
            throw new ValidationException("UserStoryId inválido.");

        // story existe e não está arquivada
        var story = await _userStoryRepository.GetByIdWithEpicIncludingArchivedAsync(dto.UserStoryId)
            ?? throw new NotFoundException("User story não encontrada.");

        if (story.IsArchived)
            throw new ConflictException("Não é possível adicionar uma user story arquivada na sprint.");

        if (story.Epic.IsArchived)
            throw new ConflictException("Não é possível adicionar uma user story de épico arquivado na sprint.");

        // garante que story pertence ao projeto
        var belongs = await _sprintItemRepository.StoryBelongsToProjectAsync(projectId, dto.UserStoryId);
        if (!belongs)
            throw new UnauthorizedAccessException("Esta user story não pertence ao projeto informado.");

        // define posição
        int position;
        if (dto.Position is null)
        {
            position = await _sprintItemRepository.GetNextPositionAsync(sprintId);
        }
        else
        {
            if (dto.Position.Value < 0)
                throw new ValidationException("Position inválida.");

            // abre espaço no destino
            await _sprintItemRepository.IncrementPositionsFromAsync(sprintId, dto.Position.Value);
            position = dto.Position.Value;
        }

        var item = new SprintItem(
            sprintId: sprintId,
            userStoryId: dto.UserStoryId,
            position: position,
            notes: dto.Notes
        );

        await _sprintItemRepository.AddAsync(item);
        await _sprintItemRepository.SaveChangesAsync();

        return MapToDto(item);
    }

    // ==========================
    // Update (reorder + notes)
    // ==========================

    public async Task<SprintItemResponseDto> UpdateAsync(Guid projectId, Guid sprintId, int sprintItemId, UpdateSprintItemDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var sprint = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        if (sprint.Status is SprintStatus.Closed or SprintStatus.Cancelled)
            throw new ConflictException("Não é possível alterar uma sprint fechada/cancelada.");

        if (dto.Position < 0)
            throw new ValidationException("Position inválida.");

        var item = await _sprintItemRepository.GetByIdAsync(projectId, sprintId, sprintItemId)
            ?? throw new NotFoundException("Item do sprint não encontrado.");

        var from = item.Position;
        var to = dto.Position;

        if (from != to)
        {
            // reordenação segura sem DEFERRABLE: usa posição temporária (max+1)
            var temp = (await _sprintItemRepository.GetMaxPositionAsync(sprintId)) + 1;

            await _sprintItemRepository.SetPositionAsync(sprintItemId, temp);
            await _sprintItemRepository.ShiftPositionsAsync(sprintId, from, to);
            await _sprintItemRepository.SetPositionAsync(sprintItemId, to);
        }

        var notes = (dto.Notes ?? string.Empty).Trim();
        await _sprintItemRepository.SetNotesAsync(sprintItemId, notes);

        // reload para devolver dto consistente
        var updated = await _sprintItemRepository.GetByIdAsync(projectId, sprintId, sprintItemId)
            ?? throw new NotFoundException("Item do sprint não encontrado.");

        return MapToDto(updated);
    }

    // ==========================
    // Delete
    // ==========================

    public async Task DeleteAsync(Guid projectId, Guid sprintId, int sprintItemId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var sprint = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        if (sprint.Status is SprintStatus.Closed or SprintStatus.Cancelled)
            throw new ConflictException("Não é possível alterar uma sprint fechada/cancelada.");

        var item = await _sprintItemRepository.GetByIdAsync(projectId, sprintId, sprintItemId)
            ?? throw new NotFoundException("Item do sprint não encontrado.");

        var removedPosition = item.Position;

        _sprintItemRepository.Remove(item);
        await _sprintItemRepository.SaveChangesAsync();

        // fecha buraco na ordenação
        await _sprintItemRepository.DecrementPositionsAfterAsync(sprintId, removedPosition);
    }

    // ==========================
    // Mapper
    // ==========================

    private static SprintItemResponseDto MapToDto(SprintItem item)
        => new(
            item.Id,
            item.SprintId,
            item.UserStoryId,
            item.Position,
            item.Notes ?? string.Empty,
            item.AddedAt
        );
}
