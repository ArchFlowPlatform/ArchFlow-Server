using archFlowServer.Models.Dtos.Board.Cards;
using archFlowServer.Models.Entities;
using archFlowServer.Models.Enums;
using archFlowServer.Models.Exceptions;
using archFlowServer.Repositories.Interfaces;

namespace archFlowServer.Services;

public class BoardCardService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ISprintRepository _sprintRepository;
    private readonly IBoardColumnRepository _columnRepository;
    private readonly IBoardCardRepository _cardRepository;
    private readonly IUserStoryRepository _userStoryRepository;
    private readonly IStoryTaskRepository _taskRepository;

    public BoardCardService(
        IProjectRepository projectRepository,
        ISprintRepository sprintRepository,
        IBoardColumnRepository columnRepository,
        IBoardCardRepository cardRepository,
        IUserStoryRepository userStoryRepository,
        IStoryTaskRepository taskRepository)
    {
        _projectRepository = projectRepository;
        _sprintRepository = sprintRepository;
        _columnRepository = columnRepository;
        _cardRepository = cardRepository;
        _userStoryRepository = userStoryRepository;
        _taskRepository = taskRepository;
    }

    public async Task<IEnumerable<BoardCardResponseDto>> GetAllByColumnAsync(Guid projectId, Guid sprintId, int columnId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        _ = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        _ = await _columnRepository.GetByIdAsync(projectId, sprintId, columnId)
            ?? throw new NotFoundException("Coluna não encontrada.");

        var cards = await _cardRepository.GetAllByColumnAsync(projectId, sprintId, columnId);
        return cards.Select(MapToDto);
    }

    public async Task<BoardCardResponseDto> CreateAsync(Guid projectId, Guid sprintId, int columnId, CreateBoardCardDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var sprint = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        if (sprint.Status is SprintStatus.Closed or SprintStatus.Cancelled)
            throw new ConflictException("Não é possível alterar board de sprint fechada/cancelada.");

        _ = await _columnRepository.GetByIdAsync(projectId, sprintId, columnId)
            ?? throw new NotFoundException("Coluna não encontrada.");

        // valida origem no nível service (cross-aggregate)
        if (dto.UserStoryId.HasValue)
        {
            _ = await EnsureUserStoryInProjectAsync(projectId, dto.UserStoryId.Value);
        }

        if (dto.StoryTaskId.HasValue)
        {
            var task = await _taskRepository.GetByIdInProjectAsync(projectId, dto.StoryTaskId.Value);
            if (task is null)
                throw new NotFoundException("Task não encontrada.");
        }


        int position;
        if (dto.Position is null)
        {
            position = await _cardRepository.GetNextPositionAsync(columnId);
        }
        else
        {
            if (dto.Position.Value < 0)
                throw new ValidationException("Position inválida.");

            await _cardRepository.IncrementPositionsFromAsync(columnId, dto.Position.Value);
            position = dto.Position.Value;
        }

        var card = new BoardCard(
            columnId: columnId,
            position: position,
            title: dto.Title,
            description: dto.Description,
            userStoryId: dto.UserStoryId,
            storyTaskId: dto.StoryTaskId,
            assigneeId: dto.AssigneeId,
            priority: dto.Priority,
            dueDate: dto.DueDate,
            estimatedHours: dto.EstimatedHours,
            color: dto.Color
        );

        await _cardRepository.AddAsync(card);
        await _cardRepository.SaveChangesAsync();

        return MapToDto(card);
    }

    public async Task<BoardCardResponseDto> UpdateAsync(Guid projectId, Guid sprintId, int cardId, UpdateBoardCardDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var sprint = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        if (sprint.Status is SprintStatus.Closed or SprintStatus.Cancelled)
            throw new ConflictException("Não é possível alterar board de sprint fechada/cancelada.");

        var card = await _cardRepository.GetByIdAsync(projectId, sprintId, cardId)
            ?? throw new NotFoundException("Card não encontrado.");

        card.Update(
            title: dto.Title,
            description: dto.Description,
            assigneeId: dto.AssigneeId,
            priority: dto.Priority,
            dueDate: dto.DueDate,
            estimatedHours: dto.EstimatedHours,
            actualHours: dto.ActualHours,
            color: dto.Color
        );

        await _cardRepository.SaveChangesAsync();
        return MapToDto(card);
    }

    public async Task MoveAsync(Guid projectId, Guid sprintId, int cardId, MoveBoardCardDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var sprint = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        if (sprint.Status is SprintStatus.Closed or SprintStatus.Cancelled)
            throw new ConflictException("Não é possível alterar board de sprint fechada/cancelada.");

        var card = await _cardRepository.GetByIdAsync(projectId, sprintId, cardId)
            ?? throw new NotFoundException("Card não encontrado.");

        var toColumn = await _columnRepository.GetByIdAsync(projectId, sprintId, dto.ToColumnId)
            ?? throw new NotFoundException("Coluna de destino não encontrada.");

        var fromColumnId = card.ColumnId;
        var fromPos = card.Position;

        // remove "buraco" na coluna origem
        await _cardRepository.DecrementPositionsAfterAsync(fromColumnId, fromPos);

        // define posição destino
        int toPos;
        if (dto.ToPosition is null)
        {
            toPos = await _cardRepository.GetNextPositionAsync(toColumn.Id);
        }
        else
        {
            if (dto.ToPosition.Value < 0)
                throw new ValidationException("ToPosition inválida.");

            await _cardRepository.IncrementPositionsFromAsync(toColumn.Id, dto.ToPosition.Value);
            toPos = dto.ToPosition.Value;
        }

        await _cardRepository.SetColumnAsync(cardId, toColumn.Id);
        await _cardRepository.SetPositionAsync(cardId, toPos);

        await _cardRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid projectId, Guid sprintId, int cardId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var sprint = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        if (sprint.Status is SprintStatus.Closed or SprintStatus.Cancelled)
            throw new ConflictException("Não é possível alterar board de sprint fechada/cancelada.");

        var card = await _cardRepository.GetByIdAsync(projectId, sprintId, cardId)
            ?? throw new NotFoundException("Card não encontrado.");

        var colId = card.ColumnId;
        var pos = card.Position;

        _cardRepository.Remove(card);
        await _cardRepository.SaveChangesAsync();

        await _cardRepository.DecrementPositionsAfterAsync(colId, pos);
    }

    private static BoardCardResponseDto MapToDto(BoardCard c)
        => new(
            c.Id, c.ColumnId, c.UserStoryId, c.StoryTaskId, c.Title, c.Description, c.AssigneeId,
            c.Position, c.Priority, c.DueDate, c.EstimatedHours, c.ActualHours, c.Color,
            c.CreatedAt, c.UpdatedAt
        );

    private async Task<UserStory> EnsureUserStoryInProjectAsync(Guid projectId, int storyId)
    {
        var story = await _userStoryRepository.GetByIdWithEpicAndBacklogAsync(storyId)
            ?? throw new NotFoundException("User story não encontrada.");

        // ProjectId vem do ProductBacklog
        if (story.Epic.ProductBacklog.ProjectId != projectId)
            throw new DomainException("Você não tem acesso a esta user story.");

        return story;
    }

}
