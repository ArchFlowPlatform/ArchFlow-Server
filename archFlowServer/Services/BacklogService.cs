using archFlowServer.Data;
using archFlowServer.Models.Dtos.Backlog;
using archFlowServer.Models.Entities;
using archFlowServer.Models.Exceptions;
using archFlowServer.Repositories.Interfaces;

namespace archFlowServer.Services;

public class BacklogService
{
    private readonly AppDbContext _context;
    private readonly IProjectRepository _projectRepository;
    private readonly IProductBacklogRepository _backlogRepository;
    private readonly IEpicRepository _epicRepository;
    private readonly IUserStoryRepository _storyRepository;

    public BacklogService(
        AppDbContext context,
        IProjectRepository projectRepository,
        IProductBacklogRepository backlogRepository,
        IEpicRepository epicRepository,
        IUserStoryRepository storyRepository)
    {
        _context = context;
        _projectRepository = projectRepository;
        _backlogRepository = backlogRepository;
        _epicRepository = epicRepository;
        _storyRepository = storyRepository;
    }

    // ================================
    // Create
    // ================================

    public async Task CreateEpicAsync(Guid projectId, CreateEpicDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
            ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        var nextPosition = await _epicRepository.GetNextPositionAsync(backlog.Id);

        var epic = new Epic(
            productBacklogId: backlog.Id,
            name: dto.Name,
            description: dto.Description,
            businessValue: dto.BusinessValue,
            status: dto.Status,
            position: nextPosition,
            priority: dto.Priority,
            color: dto.Color,
            isArchived: dto.IsArchived,
            archivedAt: null
        );

        await _epicRepository.AddAsync(epic);
        await _epicRepository.SaveChangesAsync();
    }

    public async Task CreateUserStoryAsync(Guid projectId, int epicId, CreateUserStoryDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
            ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        var epic = await _epicRepository.GetByIdAsync(epicId)
            ?? throw new NotFoundException("Épico não encontrado.");

        if (epic.ProductBacklogId != backlog.Id)
            throw new UnauthorizedAccessException("Este épico não pertence ao projeto informado.");

        var nextBacklogPosition = await _storyRepository.GetNextBacklogPositionAsync(epicId);

        var story = new UserStory(
            epicId: epicId,
            title: dto.Title,
            persona: dto.Persona,
            description: dto.Description,
            acceptanceCriteria: dto.AcceptanceCriteria,
            complexity: dto.Complexity,
            effort: dto.Effort,
            dependencies: dto.Dependencies,
            priority: dto.Priority,
            businessValue: dto.BusinessValue,
            status: dto.Status,
            backlogPosition: nextBacklogPosition, // ✅
            assigneeId: dto.AssigneeId
        );

        await _storyRepository.AddAsync(story);
        await _storyRepository.SaveChangesAsync();
    }

    // ================================
    // Read
    // ================================

    public async Task<ProductBacklogResponseDto> GetBacklogByProjectIdAsync(Guid projectId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
            ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        return new ProductBacklogResponseDto
        {
            Id = backlog.Id,
            ProjectId = backlog.ProjectId,
            Epics = backlog.Epics
                .OrderBy(e => e.Position)
                .ThenByDescending(e => e.Priority)
                .ThenBy(e => e.CreatedAt)
                .Select(e => new EpicResponseDto
                {
                    Id = e.Id,
                    ProductBacklogId = e.ProductBacklogId,
                    Name = e.Name,
                    Description = e.Description,
                    BusinessValue = e.BusinessValue,
                    Status = e.Status,
                    Position = e.Position,
                    Priority = e.Priority,
                    Color = e.Color,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,

                    UserStories = e.UserStories
                        .OrderBy(s => s.BacklogPosition) // ✅
                        .ThenByDescending(s => s.Priority)
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
                            BacklogPosition = s.BacklogPosition, // ✅
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
    }

    // ================================
    // Update
    // ================================

    public async Task UpdateOverviewAsync(Guid projectId, UpdateBacklogOverviewDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
            ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        backlog.UpdateOverview(dto.Overview);

        await _backlogRepository.SaveChangesAsync();
    }

    public async Task UpdateEpicAsync(Guid projectId, int epicId, UpdateEpicDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
            ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        var epic = await _epicRepository.GetByIdAsync(epicId)
            ?? throw new NotFoundException("Épico não encontrado.");

        if (epic.ProductBacklogId != backlog.Id)
            throw new UnauthorizedAccessException("Este épico não pertence ao projeto informado.");

        epic.Update(
            name: dto.Name ?? epic.Name,
            description: dto.Description ?? epic.Description,
            businessValue: dto.BusinessValue ?? epic.BusinessValue,
            status: dto.Status ?? epic.Status,
            position: dto.Position ?? epic.Position,
            priority: dto.Priority ?? epic.Priority,
            color: dto.Color ?? epic.Color
        );

        await _epicRepository.SaveChangesAsync();
    }

    public async Task UpdateUserStoryAsync(Guid projectId, int storyId, UpdateUserStoryDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
            ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        var story = await _storyRepository.GetByIdWithEpicAsync(storyId)
            ?? throw new NotFoundException("User story não encontrada.");

        if (story.Epic.ProductBacklogId != backlog.Id)
            throw new UnauthorizedAccessException("Esta user story não pertence ao projeto informado.");

        story.Update(
            title: dto.Title ?? story.Title,
            persona: dto.Persona ?? story.Persona,
            description: dto.Description ?? story.Description,
            acceptanceCriteria: dto.AcceptanceCriteria ?? story.AcceptanceCriteria,
            complexity: dto.Complexity ?? story.Complexity,
            effort: dto.Effort ?? story.Effort,
            dependencies: dto.Dependencies ?? story.Dependencies,
            priority: dto.Priority ?? story.Priority,
            businessValue: dto.BusinessValue ?? story.BusinessValue,
            status: dto.Status ?? story.Status,
            backlogPosition: dto.BacklogPosition ?? story.BacklogPosition, // ✅
            assigneeId: dto.AssigneeId ?? story.AssigneeId
        );

        await _storyRepository.SaveChangesAsync();
    }

    // ================================
    // Reorder / Move (BACKLOG)
    // ================================

    public async Task ReorderEpicAsync(Guid projectId, ReorderEpicDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
            ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        var epic = await _epicRepository.GetByIdAsync(dto.EpicId)
            ?? throw new NotFoundException("Épico não encontrado.");

        if (epic.ProductBacklogId != backlog.Id)
            throw new UnauthorizedAccessException("Este épico não pertence ao projeto informado.");

        if (dto.ToPosition < 0)
            throw new ValidationException("ToPosition inválido.");

        var from = epic.Position;

        var maxPos = await _epicRepository.GetMaxPositionAsync(backlog.Id);
        var to = dto.ToPosition > maxPos ? maxPos : dto.ToPosition;

        if (from == to) return;

        var temp = maxPos + 1;

        await using var tx = await _context.Database.BeginTransactionAsync();

        await _epicRepository.SetPositionAsync(epic.Id, temp);
        await _epicRepository.ShiftPositionsAsync(backlog.Id, from, to);
        await _epicRepository.SetPositionAsync(epic.Id, to);

        await tx.CommitAsync();
    }

    public async Task ReorderUserStoryAsync(Guid projectId, ReorderUserStoryDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
            ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        var story = await _storyRepository.GetByIdWithEpicAsync(dto.StoryId)
            ?? throw new NotFoundException("User story não encontrada.");

        if (story.Epic.ProductBacklogId != backlog.Id)
            throw new UnauthorizedAccessException("Esta user story não pertence ao projeto informado.");

        if (dto.ToPosition < 0)
            throw new ValidationException("ToPosition inválido.");

        var epicId = story.EpicId;
        var from = story.BacklogPosition; // ✅

        var maxPos = await _storyRepository.GetMaxBacklogPositionAsync(epicId);
        var to = dto.ToPosition > maxPos ? maxPos : dto.ToPosition;

        if (from == to) return;

        var temp = maxPos + 1;

        await using var tx = await _context.Database.BeginTransactionAsync();

        await _storyRepository.SetBacklogPositionAsync(story.Id, temp);
        await _storyRepository.ShiftBacklogPositionsAsync(epicId, from, to);
        await _storyRepository.SetBacklogPositionAsync(story.Id, to);

        await tx.CommitAsync();
    }

    public async Task MoveUserStoryAsync(Guid projectId, MoveUserStoryDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        if (dto.ToPosition < 0)
            throw new ValidationException("ToPosition inválido.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
            ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        var story = await _storyRepository.GetByIdWithEpicAsync(dto.StoryId)
            ?? throw new NotFoundException("User story não encontrada.");

        if (story.Epic.ProductBacklogId != backlog.Id)
            throw new UnauthorizedAccessException("Esta user story não pertence ao projeto informado.");

        var toEpic = await _epicRepository.GetByIdAsync(dto.ToEpicId)
            ?? throw new NotFoundException("Épico de destino não encontrado.");

        if (toEpic.ProductBacklogId != backlog.Id)
            throw new UnauthorizedAccessException("O épico de destino não pertence ao projeto informado.");

        var fromEpicId = story.EpicId;
        var fromPosition = story.BacklogPosition; // ✅
        var toEpicId = toEpic.Id;

        // mesmo epic -> reorder
        if (fromEpicId == toEpicId)
        {
            await ReorderUserStoryAsync(projectId, new ReorderUserStoryDto
            {
                StoryId = dto.StoryId,
                ToPosition = dto.ToPosition
            });
            return;
        }

        // destino pode inserir no final também:
        var maxDest = await _storyRepository.GetMaxBacklogPositionAsync(toEpicId);
        var destCount = maxDest + 1;
        var toPosition = dto.ToPosition > destCount ? destCount : dto.ToPosition;

        var maxFrom = await _storyRepository.GetMaxBacklogPositionAsync(fromEpicId);
        var temp = maxFrom + 1;

        await using var tx = await _context.Database.BeginTransactionAsync();

        // tira da posição original sem quebrar unique index (EpicId, BacklogPosition)
        await _storyRepository.SetBacklogPositionAsync(story.Id, temp);

        // fecha buraco no epic origem
        await _storyRepository.DecrementBacklogPositionsAfterAsync(fromEpicId, fromPosition);

        // abre espaço no epic destino
        await _storyRepository.IncrementBacklogPositionsFromAsync(toEpicId, toPosition);

        // move epic + backlogPosition final
        await _storyRepository.SetEpicAndBacklogPositionAsync(story.Id, toEpicId, toPosition);

        await tx.CommitAsync();
    }

    // ================================
    // Archive / Restore
    // ================================

    public async Task ArchiveEpicAsync(Guid projectId, int epicId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
            ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        var epic = await _epicRepository.GetByIdAsync(epicId)
            ?? throw new NotFoundException("Épico não encontrado.");

        if (epic.ProductBacklogId != backlog.Id)
            throw new UnauthorizedAccessException("Este épico não pertence ao projeto informado.");

        await using var tx = await _context.Database.BeginTransactionAsync();

        await _epicRepository.ArchiveAsync(epicId);
        await _storyRepository.ArchiveByEpicIdAsync(epicId);

        await tx.CommitAsync();
    }

    public async Task RestoreEpicAsync(Guid projectId, int epicId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
            ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        var epic = await _epicRepository.GetByIdIncludingArchivedAsync(epicId)
            ?? throw new NotFoundException("Épico não encontrado.");

        if (epic.ProductBacklogId != backlog.Id)
            throw new UnauthorizedAccessException("Este épico não pertence ao projeto informado.");

        await using var tx = await _context.Database.BeginTransactionAsync();

        await _epicRepository.RestoreAsync(epicId);
        await _storyRepository.RestoreByEpicIdAsync(epicId);

        await tx.CommitAsync();
    }

    public async Task ArchiveUserStoryAsync(Guid projectId, int storyId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
            ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        var story = await _storyRepository.GetByIdWithEpicAsync(storyId)
            ?? throw new NotFoundException("User story não encontrada.");

        if (story.Epic.ProductBacklogId != backlog.Id)
            throw new UnauthorizedAccessException("Esta user story não pertence ao projeto informado.");

        await _storyRepository.ArchiveAsync(storyId);
        await _storyRepository.SaveChangesAsync(); // ✅ faltava
    }

    public async Task RestoreUserStoryAsync(Guid projectId, int storyId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
            ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        var story = await _storyRepository.GetByIdWithEpicIncludingArchivedAsync(storyId)
            ?? throw new NotFoundException("User story não encontrada.");

        if (story.Epic.ProductBacklogId != backlog.Id)
            throw new UnauthorizedAccessException("Esta user story não pertence ao projeto informado.");

        await _storyRepository.RestoreAsync(storyId);
        await _storyRepository.SaveChangesAsync(); // ✅ faltava
    }
}
