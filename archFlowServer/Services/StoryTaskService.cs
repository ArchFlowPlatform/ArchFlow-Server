using archFlowServer.Models.Dtos.Task;
using archFlowServer.Models.Entities;
using archFlowServer.Models.Exceptions;
using archFlowServer.Repositories.Interfaces;

namespace archFlowServer.Services;

public class StoryTaskService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUserStoryRepository _userStoryRepository;
    private readonly IStoryTaskRepository _taskRepository;

    public StoryTaskService(
        IProjectRepository projectRepository,
        IUserStoryRepository userStoryRepository,
        IStoryTaskRepository taskRepository)
    {
        _projectRepository = projectRepository;
        _userStoryRepository = userStoryRepository;
        _taskRepository = taskRepository;
    }

    public async Task<IEnumerable<StoryTaskResponseDto>> GetAllAsync(Guid projectId, int userStoryId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        _ = await EnsureUserStoryInProjectAsync(projectId, userStoryId);

        var tasks = await _taskRepository.GetAllByUserStoryAsync(projectId, userStoryId);
        return tasks.Select(MapToDto);
    }

    public async Task<StoryTaskResponseDto> CreateAsync(Guid projectId, int userStoryId, CreateStoryTaskDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        _ = await EnsureUserStoryInProjectAsync(projectId, userStoryId);

        var task = new StoryTask(
            userStoryId: userStoryId,
            title: dto.Title,
            description: dto.Description,
            assigneeId: dto.AssigneeId,
            estimatedHours: dto.EstimatedHours,
            priority: dto.Priority
        );

        await _taskRepository.AddAsync(task);
        await _taskRepository.SaveChangesAsync();

        return MapToDto(task);
    }

    public async Task<StoryTaskResponseDto> UpdateAsync(Guid projectId, int userStoryId, int taskId, UpdateStoryTaskDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        _ = await EnsureUserStoryInProjectAsync(projectId, userStoryId);

        var task = await _taskRepository.GetByIdAsync(projectId, userStoryId, taskId)
            ?? throw new NotFoundException("Task não encontrada.");

        task.Update(
            title: dto.Title,
            description: dto.Description,
            assigneeId: dto.AssigneeId,
            estimatedHours: dto.EstimatedHours,
            actualHours: dto.ActualHours,
            priority: dto.Priority
        );

        await _taskRepository.SaveChangesAsync();
        return MapToDto(task);
    }

    public async Task DeleteAsync(Guid projectId, int userStoryId, int taskId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        _ = await EnsureUserStoryInProjectAsync(projectId, userStoryId);

        var task = await _taskRepository.GetByIdAsync(projectId, userStoryId, taskId)
            ?? throw new NotFoundException("Task não encontrada.");

        _taskRepository.Remove(task);
        await _taskRepository.SaveChangesAsync();
    }

    // ==========================
    // Helpers
    // ==========================

    private async Task<UserStory> EnsureUserStoryInProjectAsync(Guid projectId, int storyId)
    {
        var story = await _userStoryRepository.GetByIdWithEpicAndBacklogAsync(storyId)
            ?? throw new NotFoundException("User story não encontrada.");

        // aqui depende do seu modelo:
        // story.Epic.ProductBacklog.ProjectId
        var storyProjectId = story.Epic.ProductBacklog.ProjectId;

        if (storyProjectId != projectId)
            throw new DomainException("Você não tem acesso a esta user story.");

        return story;
    }

    private static StoryTaskResponseDto MapToDto(StoryTask t)
        => new(
            t.Id,
            t.UserStoryId,
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
