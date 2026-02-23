using archFlowServer.Data;
using archFlowServer.Models.Dtos.Task;
using archFlowServer.Models.Entities;
using archFlowServer.Models.Exceptions;
using archFlowServer.Repositories.Interfaces;
using ArchFlowServer.Models.Dtos.Task;

namespace archFlowServer.Services;

public class StoryTaskService
{
    private readonly AppDbContext _context;
    private readonly IProjectRepository _projectRepository;
    private readonly ISprintRepository _sprintRepository;
    private readonly ISprintItemRepository _sprintItemRepository;
    private readonly IStoryTaskRepository _taskRepository;

    public StoryTaskService(
        AppDbContext context,
        IProjectRepository projectRepository,
        ISprintRepository sprintRepository,
        ISprintItemRepository sprintItemRepository,
        IStoryTaskRepository taskRepository)
    {
        _context = context;
        _projectRepository = projectRepository;
        _sprintRepository = sprintRepository;
        _sprintItemRepository = sprintItemRepository;
        _taskRepository = taskRepository;
    }

    public async Task<IEnumerable<StoryTaskResponseDto>> GetAllAsync(Guid projectId, Guid sprintId, int sprintItemId)
    {
        var sprintItem = await EnsureProjectSprintAndItemAsync(projectId, sprintId, sprintItemId);

        var tasks = await _taskRepository.GetAllBySprintItemAsync(projectId, sprintItemId);
        return tasks.Select(t => MapToDto(t, sprintItem.UserStoryId));
    }

    public async Task<StoryTaskResponseDto> CreateAsync(Guid projectId, Guid sprintId, int sprintItemId, CreateStoryTaskDto dto)
    {
        var sprintItem = await EnsureProjectSprintAndItemAsync(projectId, sprintId, sprintItemId);

        var nextPosition = await _taskRepository.GetNextPositionAsync(sprintItemId);

        var task = new StoryTask(
            sprintItemId: sprintItemId,
            title: dto.Title,
            description: dto.Description,
            assigneeId: dto.AssigneeId,
            estimatedHours: dto.EstimatedHours,
            priority: dto.Priority,
            position: nextPosition,
            status: StoryTaskStatus.Todo
        );

        await _taskRepository.AddAsync(task);
        await _taskRepository.SaveChangesAsync();

        return MapToDto(task, sprintItem.UserStoryId);
    }

    public async Task<StoryTaskResponseDto> UpdateAsync(Guid projectId, Guid sprintId, int sprintItemId, int taskId, UpdateStoryTaskDto dto)
    {
        var sprintItem = await EnsureProjectSprintAndItemAsync(projectId, sprintId, sprintItemId);

        var task = await _taskRepository.GetByIdAsync(projectId, sprintItemId, taskId)
            ?? throw new NotFoundException("Task não encontrada.");

        task.Update(
            title: dto.Title,
            description: dto.Description,
            assigneeId: dto.AssigneeId,
            estimatedHours: dto.EstimatedHours,
            actualHours: dto.ActualHours,
            priority: dto.Priority,
            status: dto.Status
        );

        await _taskRepository.SaveChangesAsync();
        return MapToDto(task, sprintItem.UserStoryId);
    }

    public async Task DeleteAsync(Guid projectId, Guid sprintId, int sprintItemId, int taskId)
    {
        _ = await EnsureProjectSprintAndItemAsync(projectId, sprintId, sprintItemId);

        var task = await _taskRepository.GetByIdAsync(projectId, sprintItemId, taskId)
            ?? throw new NotFoundException("Task não encontrada.");

        var removedPos = task.Position;

        await using var tx = await _context.Database.BeginTransactionAsync();

        _taskRepository.Remove(task);
        await _taskRepository.SaveChangesAsync();

        await _taskRepository.DecrementPositionsAfterAsync(sprintItemId, removedPos);

        await tx.CommitAsync();
    }

    public async Task ReorderAsync(Guid projectId, Guid sprintId, int sprintItemId, ReorderStoryTaskDto dto)
    {
        _ = await EnsureProjectSprintAndItemAsync(projectId, sprintId, sprintItemId);

        if (dto.ToPosition < 0)
            throw new ValidationException("ToPosition inválido.");

        var task = await _taskRepository.GetByIdAsync(projectId, sprintItemId, dto.TaskId)
            ?? throw new NotFoundException("Task não encontrada.");

        var from = task.Position;

        var maxPos = await _taskRepository.GetMaxPositionAsync(sprintItemId);
        var to = dto.ToPosition > maxPos ? maxPos : dto.ToPosition;

        if (from == to) return;

        var temp = maxPos + 1;

        await using var tx = await _context.Database.BeginTransactionAsync();

        await _taskRepository.SetPositionAsync(task.Id, temp);
        await _taskRepository.ShiftPositionsAsync(sprintItemId, from, to);
        await _taskRepository.SetPositionAsync(task.Id, to);

        await tx.CommitAsync();
    }

    public async Task MoveAsync(Guid projectId, Guid sprintId, int fromSprintItemId, MoveStoryTaskDto dto)
    {
        var fromItem = await EnsureProjectSprintAndItemAsync(projectId, sprintId, fromSprintItemId);
        var toItem = await EnsureProjectSprintAndItemAsync(projectId, sprintId, dto.ToSprintItemId);

        if (dto.ToPosition < 0)
            throw new ValidationException("ToPosition inválido.");

        var task = await _taskRepository.GetByIdAsync(projectId, fromSprintItemId, dto.TaskId)
            ?? throw new NotFoundException("Task não encontrada.");

        var fromPos = task.Position;

        if (fromSprintItemId == dto.ToSprintItemId)
        {
            await ReorderAsync(projectId, sprintId, fromSprintItemId, new ReorderStoryTaskDto
            {
                TaskId = dto.TaskId,
                ToPosition = dto.ToPosition
            });
            return;
        }

        var maxDest = await _taskRepository.GetMaxPositionAsync(dto.ToSprintItemId);
        var destCount = maxDest + 1;
        var toPosition = dto.ToPosition > destCount ? destCount : dto.ToPosition;

        var maxFrom = await _taskRepository.GetMaxPositionAsync(fromSprintItemId);
        var temp = maxFrom + 1;

        await using var tx = await _context.Database.BeginTransactionAsync();

        await _taskRepository.SetPositionAsync(task.Id, temp);

        await _taskRepository.DecrementPositionsAfterAsync(fromSprintItemId, fromPos);
        await _taskRepository.IncrementPositionsFromAsync(dto.ToSprintItemId, toPosition);

        await _taskRepository.SetSprintItemAndPositionAsync(task.Id, dto.ToSprintItemId, toPosition);

        await tx.CommitAsync();

        // (MapToDto não é necessário aqui porque o endpoint retorna Ok sem data)
        _ = fromItem; _ = toItem;
    }

    // ==========================
    // Validation (core)
    // ==========================
    private async Task<SprintItem> EnsureProjectSprintAndItemAsync(Guid projectId, Guid sprintId, int sprintItemId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var sprint = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        if (sprint.ProjectId != projectId)
            throw new DomainException("Você não tem acesso a esta sprint.");

        var item = await _sprintItemRepository.GetByIdAsync(projectId, sprintId, sprintItemId)
            ?? throw new NotFoundException("Sprint item não encontrado.");

        if (item.SprintId != sprintId)
            throw new DomainException("Este item não pertence à sprint informada.");

        return item;
    }

    private static StoryTaskResponseDto MapToDto(StoryTask t, int userStoryId)
        => new(
            t.Id,
            userStoryId,          // ✅ mantém compatibilidade de frontend: “task pertence a story”
            t.Position,
            t.Title,
            t.Description,
            t.AssigneeId,
            t.EstimatedHours,
            t.ActualHours,
            t.Priority,
            t.CreatedAt,
            t.UpdatedAt
        );
}