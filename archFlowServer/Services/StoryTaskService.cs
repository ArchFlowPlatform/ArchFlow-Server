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
    private readonly IUserStoryRepository _userStoryRepository;
    private readonly IStoryTaskRepository _taskRepository;

    public StoryTaskService(
        AppDbContext context,
        IProjectRepository projectRepository,
        IUserStoryRepository userStoryRepository,
        IStoryTaskRepository taskRepository)
    {
        _context = context;
        _projectRepository = projectRepository;
        _userStoryRepository = userStoryRepository;
        _taskRepository = taskRepository;
    }

    public async Task<IEnumerable<StoryTaskResponseDto>> GetAllAsync(Guid projectId, int userStoryId)
    {
        await EnsureProjectAndStoryAsync(projectId, userStoryId);

        var tasks = await _taskRepository.GetAllByUserStoryAsync(projectId, userStoryId);
        return tasks.Select(MapToDto);
    }

    public async Task<StoryTaskResponseDto> CreateAsync(Guid projectId, int userStoryId, CreateStoryTaskDto dto)
    {
        await EnsureProjectAndStoryAsync(projectId, userStoryId);

        var nextPosition = await _taskRepository.GetNextPositionAsync(userStoryId);

        var task = new StoryTask(
            userStoryId: userStoryId,
            title: dto.Title,
            description: dto.Description,
            assigneeId: dto.AssigneeId,
            position: nextPosition,
            estimatedHours: dto.EstimatedHours,
            priority: dto.Priority,
            status: StoryTaskStatus.Todo
        );

        await _taskRepository.AddAsync(task);
        await _taskRepository.SaveChangesAsync();

        return MapToDto(task);
    }

    public async Task<StoryTaskResponseDto> UpdateAsync(Guid projectId, int userStoryId, int taskId, UpdateStoryTaskDto dto)
    {
        await EnsureProjectAndStoryAsync(projectId, userStoryId);

        var task = await _taskRepository.GetByIdAsync(projectId, userStoryId, taskId)
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
        return MapToDto(task);
    }

    public async Task DeleteAsync(Guid projectId, int userStoryId, int taskId)
    {
        await EnsureProjectAndStoryAsync(projectId, userStoryId);

        var task = await _taskRepository.GetByIdAsync(projectId, userStoryId, taskId)
            ?? throw new NotFoundException("Task não encontrada.");

        // ✅ fecha buraco na lista
        var removedPos = task.Position;

        await using var tx = await _context.Database.BeginTransactionAsync();

        _taskRepository.Remove(task);
        await _taskRepository.SaveChangesAsync();

        await _taskRepository.DecrementPositionsAfterAsync(userStoryId, removedPos);

        await tx.CommitAsync();
    }

    // ================================
    // Drag reorder (mesma user story)
    // ================================

    public async Task ReorderAsync(Guid projectId, int userStoryId, ReorderStoryTaskDto dto)
    {
        await EnsureProjectAndStoryAsync(projectId, userStoryId);

        if (dto.ToPosition < 0)
            throw new ValidationException("ToPosition inválido.");

        var task = await _taskRepository.GetByIdAsync(projectId, userStoryId, dto.TaskId)
            ?? throw new NotFoundException("Task não encontrada.");

        var from = task.Position;

        var maxPos = await _taskRepository.GetMaxPositionAsync(userStoryId);
        var to = dto.ToPosition > maxPos ? maxPos : dto.ToPosition;

        if (from == to) return;

        var temp = maxPos + 1;

        await using var tx = await _context.Database.BeginTransactionAsync();

        await _taskRepository.SetPositionAsync(task.Id, temp);
        await _taskRepository.ShiftPositionsAsync(userStoryId, from, to);
        await _taskRepository.SetPositionAsync(task.Id, to);

        await tx.CommitAsync();
    }

    // ================================
    // Drag move (entre user stories)
    // ================================

    public async Task MoveAsync(Guid projectId, int fromUserStoryId, MoveStoryTaskDto dto)
    {
        await EnsureProjectAndStoryAsync(projectId, fromUserStoryId);
        await EnsureProjectAndStoryAsync(projectId, dto.ToUserStoryId);

        if (dto.ToPosition < 0)
            throw new ValidationException("ToPosition inválido.");

        var task = await _taskRepository.GetByIdAsync(projectId, fromUserStoryId, dto.TaskId)
            ?? throw new NotFoundException("Task não encontrada.");

        var fromPos = task.Position;

        // mesmo container? vira reorder
        if (fromUserStoryId == dto.ToUserStoryId)
        {
            await ReorderAsync(projectId, fromUserStoryId, new ReorderStoryTaskDto
            {
                TaskId = dto.TaskId,
                ToPosition = dto.ToPosition
            });
            return;
        }

        var maxDest = await _taskRepository.GetMaxPositionAsync(dto.ToUserStoryId);
        var destCount = maxDest + 1;
        var toPosition = dto.ToPosition > destCount ? destCount : dto.ToPosition;

        var maxFrom = await _taskRepository.GetMaxPositionAsync(fromUserStoryId);
        var temp = maxFrom + 1;

        await using var tx = await _context.Database.BeginTransactionAsync();

        // tira do range do "from" sem quebrar unique
        await _taskRepository.SetPositionAsync(task.Id, temp);

        // fecha buraco no from
        await _taskRepository.DecrementPositionsAfterAsync(fromUserStoryId, fromPos);

        // abre espaço no destino
        await _taskRepository.IncrementPositionsFromAsync(dto.ToUserStoryId, toPosition);

        // move para destino
        await _taskRepository.SetUserStoryAndPositionAsync(task.Id, dto.ToUserStoryId, toPosition);

        await tx.CommitAsync();
    }

    // ==========================
    // Helpers
    // ==========================

    private async Task<UserStory> EnsureProjectAndStoryAsync(Guid projectId, int storyId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var story = await _userStoryRepository.GetByIdWithEpicAndBacklogAsync(storyId)
            ?? throw new NotFoundException("User story não encontrada.");

        var storyProjectId = story.Epic.ProductBacklog.ProjectId;
        if (storyProjectId != projectId)
            throw new DomainException("Você não tem acesso a esta user story.");

        return story;
    }

    private static StoryTaskResponseDto MapToDto(StoryTask t)
        => new(
            t.Id,
            t.UserStoryId,
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
