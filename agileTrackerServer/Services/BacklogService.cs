using agileTrackerServer.Models.Dtos.Backlog;
using agileTrackerServer.Models.Entities;
using agileTrackerServer.Models.Exceptions;
using agileTrackerServer.Repositories.Interfaces;

namespace agileTrackerServer.Services;

public class BacklogService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProductBacklogRepository _backlogRepository;
    private readonly IEpicRepository _epicRepository;
    private readonly IUserStoryRepository _storyRepository;

    public BacklogService(
        IProjectRepository projectRepository,
        IProductBacklogRepository backlogRepository,
        IEpicRepository epicRepository,
        IUserStoryRepository storyRepository)
    {
        _projectRepository = projectRepository;
        _backlogRepository = backlogRepository;
        _epicRepository = epicRepository;
        _storyRepository = storyRepository;
    }

    public async Task CreateEpicAsync(Guid projectId, Guid userId, CreateEpicDto dto)
    {
        // valida acesso ao projeto
        var project = await _projectRepository.GetByIdAsync(projectId, userId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
            ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        var epic = new Epic(
            backlog.Id,
            dto.Name,
            dto.Description,
            dto.BusinessValue,
            dto.Status,
            dto.Priority,
            dto.Color
        );

        await _epicRepository.AddAsync(epic);
        await _epicRepository.SaveChangesAsync();
    }

    public async Task CreateUserStoryAsync(Guid projectId, Guid epicId, Guid userId, CreateUserStoryDto dto)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, userId)
                      ?? throw new NotFoundException("Projeto não encontrado.");

        var epic = await _epicRepository.GetByIdAsync(epicId)
            ?? throw new NotFoundException("Épico não encontrado.");

        // opcional: garantir que o epic pertence ao backlog do projeto
        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
            ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        if (epic.ProductBacklogId != backlog.Id)
            throw new UnauthorizedAccessException("Este épico não pertence ao projeto informado.");

        var story = new UserStory(
            epicId,
            dto.Title,
            dto.Persona,
            dto.Description,
            dto.AcceptanceCriteria,
            dto.Complexity,
            dto.Effort,
            dto.Dependencies,
            dto.Priority,
            dto.BusinessValue,
            dto.Status,
            dto.AssigneeId
        );

        await _storyRepository.AddAsync(story);
        await _storyRepository.SaveChangesAsync();
    }
    
    public async Task<ProductBacklogResponseDto> GetBacklogByProjectIdAsync(Guid projectId, Guid userId)
    {
        // 1) Segurança: usuário precisa ter acesso ao projeto
        _ = await _projectRepository.GetByIdAsync(projectId, userId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        // 2) Buscar backlog agregado (Include Epics + Stories)
        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
            ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        // 3) Mapear para DTO (com ordenação para o front)
        var dto = new ProductBacklogResponseDto
        {
            Id = backlog.Id,
            ProjectId = backlog.ProjectId,
            Overview = backlog.Overview,
            CreatedAt = backlog.CreatedAt,
            UpdatedAt = backlog.UpdatedAt,
            Epics = backlog.Epics
                .OrderByDescending(e => e.Priority)
                .ThenBy(e => e.CreatedAt)
                .Select(e => new EpicResponseDto
                {
                    Id = e.Id,
                    ProductBacklogId = e.ProductBacklogId,
                    Name = e.Name,
                    Description = e.Description,
                    BusinessValue = e.BusinessValue,
                    Status = e.Status,
                    Priority = e.Priority,
                    Color = e.Color,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                    UserStories = e.UserStories
                        .OrderByDescending(s => s.Priority)
                        .ThenBy(s => s.CreatedAt)
                        .Select(s => new UserStoryResponseDto
                        {
                            Id = s.Id,
                            EpicId = s.EpicId,
                            Title = s.Title,
                            Persona = s.Persona,
                            Description = s.Description,
                            AcceptanceCriteria = s.AcceptanceCriteria,
                            Complexity = s.Complexity,
                            Effort = s.Effort,
                            Dependencies = s.Dependencies,
                            Priority = s.Priority,
                            BusinessValue = s.BusinessValue,
                            Status = s.Status,
                            AssigneeId = s.AssigneeId,
                            CreatedAt = s.CreatedAt,
                            UpdatedAt = s.UpdatedAt
                        })
                        .ToList()
                })
                .ToList()
        };

        return dto;
    }
}
