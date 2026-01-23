using archFlowServer.Data;
using archFlowServer.Models.Dtos.Backlog;
using archFlowServer.Models.Entities;
using archFlowServer.Models.Exceptions;
using archFlowServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    public async Task CreateEpicAsync(Guid projectId, Guid userId, CreateEpicDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId, userId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
            ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        // âœ… Position Ã© sempre calculado no backend (append)
        var nextPosition = await _epicRepository.GetNextPositionAsync(backlog.Id);

        var epic = new Epic(
            backlog.Id,
            dto.Name,
            dto.Description,
            dto.BusinessValue,
            dto.Status,
            position: nextPosition,
            dto.Priority,
            dto.Color,
            dto.IsArchived,
            null
        );

        await _epicRepository.AddAsync(epic);
        await _epicRepository.SaveChangesAsync();
    }

    public async Task CreateUserStoryAsync(Guid projectId, int epicId, Guid userId, CreateUserStoryDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId, userId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var epic = await _epicRepository.GetByIdAsync(epicId)
            ?? throw new NotFoundException("Ã‰pico não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
            ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        if (epic.ProductBacklogId != backlog.Id)
            throw new UnauthorizedAccessException("Este Ã©pico não pertence ao projeto informado.");

        // âœ… Position Ã© sempre calculado no backend (append)
        var nextPosition = await _storyRepository.GetNextPositionAsync(epicId);

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
            position: nextPosition,
            dto.AssigneeId,
            dto.IsArchived, 
            null
        );

        await _storyRepository.AddAsync(story);
        await _storyRepository.SaveChangesAsync();
    }

    public async Task<ProductBacklogResponseDto> GetBacklogByProjectIdAsync(Guid projectId, Guid userId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId, userId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
            ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        var dto = new ProductBacklogResponseDto
        {
            Id = backlog.Id,
            ProjectId = backlog.ProjectId,
            Overview = backlog.Overview,
            CreatedAt = backlog.CreatedAt,
            UpdatedAt = backlog.UpdatedAt,

            // âœ… OrdenaÃ§Ã£o por Position (UI) primeiro
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
                        .OrderBy(s => s.Position)
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
                            Position = s.Position,
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

    public async Task UpdateOverviewAsync(Guid projectId, Guid userId, UpdateBacklogOverviewDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId, userId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
            ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        backlog.UpdateOverview(dto.Overview);

        await _backlogRepository.SaveChangesAsync();
    }

    public async Task UpdateEpicAsync(Guid projectId, int epicId, Guid userId, UpdateEpicDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId, userId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
            ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        var epic = await _epicRepository.GetByIdAsync(epicId)
            ?? throw new NotFoundException("Ã‰pico não encontrado.");

        if (epic.ProductBacklogId != backlog.Id)
            throw new UnauthorizedAccessException("Este Ã©pico não pertence ao projeto informado.");

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

    public async Task UpdateUserStoryAsync(Guid projectId, int storyId, Guid userId, UpdateUserStoryDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId, userId)
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
            position: dto.Position ?? story.Position,
            assigneeId: dto.AssigneeId ?? story.AssigneeId
        );

        await _storyRepository.SaveChangesAsync();
    }
    public async Task ReorderEpicAsync(Guid projectId, Guid userId, ReorderEpicDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId, userId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
                      ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        var epic = await _epicRepository.GetByIdAsync(dto.EpicId)
                   ?? throw new NotFoundException("Ã‰pico não encontrado.");

        if (epic.ProductBacklogId != backlog.Id)
            throw new UnauthorizedAccessException("Este Ã©pico não pertence ao projeto informado.");

        if (dto.ToPosition < 0)
            throw new ValidationException("ToPosition inválido.");

        var from = epic.Position;

        // limita ToPosition ao intervalo existente
        var maxPos = await _epicRepository.GetMaxPositionAsync(backlog.Id);
        var to = dto.ToPosition > maxPos ? maxPos : dto.ToPosition;

        if (from == to) return;

        // posiÃ§Ã£o temporÃ¡ria livre (fora do range)
        var temp = maxPos + 1;

        // âš ï¸ use transaÃ§Ã£o do DbContext (mesmo contexto do repo)
        using var tx = await _context.Database.BeginTransactionAsync();

        // 1) tira o Ã©pico da linha (evita conflito no UNIQUE)
        await _epicRepository.SetPositionAsync(epic.Id, temp);

        // 2) shift faixa
        await _epicRepository.ShiftPositionsAsync(backlog.Id, from, to);

        // 3) coloca no destino
        await _epicRepository.SetPositionAsync(epic.Id, to);

        await tx.CommitAsync();
    }


    public async Task ReorderUserStoryAsync(Guid projectId, Guid userId, ReorderUserStoryDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId, userId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
                      ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        var story = await _storyRepository.GetByIdWithEpicAsync(dto.StoryId)
                    ?? throw new NotFoundException("User story não encontrada.");

        // garante que a story pertence ao backlog do projeto
        if (story.Epic.ProductBacklogId != backlog.Id)
            throw new UnauthorizedAccessException("Esta user story não pertence ao projeto informado.");

        if (dto.ToPosition < 0)
            throw new ValidationException("ToPosition inválido.");

        var epicId = story.EpicId;
        var from = story.Position;

        // clamp ToPosition para o range existente
        var maxPos = await _storyRepository.GetMaxPositionAsync(epicId);
        var to = dto.ToPosition > maxPos ? maxPos : dto.ToPosition;

        if (from == to) return;

        // posiÃ§Ã£o temporÃ¡ria fora do range para não violar UNIQUE(EpicId, Position)
        var temp = maxPos + 1;

        await using var tx = await _context.Database.BeginTransactionAsync();

        // 1) tira a story da linha
        await _storyRepository.SetPositionAsync(story.Id, temp);

        // 2) shift intervalo
        await _storyRepository.ShiftPositionsAsync(epicId, from, to);

        // 3) coloca no destino
        await _storyRepository.SetPositionAsync(story.Id, to);

        await tx.CommitAsync();
    }
    
    public async Task MoveUserStoryAsync(Guid projectId, Guid userId, MoveUserStoryDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId, userId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        if (dto.ToPosition < 0)
            throw new ValidationException("ToPosition inválido.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
            ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        // story + epic origem (Include)
        var story = await _storyRepository.GetByIdWithEpicAsync(dto.StoryId)
            ?? throw new NotFoundException("User story não encontrada.");

        // valida origem no projeto
        if (story.Epic.ProductBacklogId != backlog.Id)
            throw new UnauthorizedAccessException("Esta user story não pertence ao projeto informado.");

        // epic destino
        var toEpic = await _epicRepository.GetByIdAsync(dto.ToEpicId)
            ?? throw new NotFoundException("Ã‰pico de destino não encontrado.");

        // valida destino no mesmo projeto/backlog
        if (toEpic.ProductBacklogId != backlog.Id)
            throw new UnauthorizedAccessException("O Ã©pico de destino não pertence ao projeto informado.");

        var fromEpicId = story.EpicId;
        var fromPosition = story.Position;
        var toEpicId = toEpic.Id;

        // Se o destino Ã© o mesmo Ã©pico, use o reorder normal
        if (fromEpicId == toEpicId)
        {
            await ReorderUserStoryAsync(projectId, userId, new ReorderUserStoryDto
            {
                StoryId = dto.StoryId,
                ToPosition = dto.ToPosition
            });
            return;
        }

        // clamp da posiÃ§Ã£o de destino para [0..max+1] (append permitido)
        var maxDest = await _storyRepository.GetMaxPositionAsync(toEpicId);
        var destCount = maxDest + 1;
        var toPosition = dto.ToPosition > destCount ? destCount : dto.ToPosition;

        // posiÃ§Ã£o temporÃ¡ria segura (no Ã©pico origem) para evitar colisÃµes no UNIQUE durante a operaÃ§Ã£o
        var maxFrom = await _storyRepository.GetMaxPositionAsync(fromEpicId);
        var temp = maxFrom + 1;

        await using var tx = await _context.Database.BeginTransactionAsync();

        // 1) tira a story da posiÃ§Ã£o original (manda para temp dentro do Ã©pico origem)
        await _storyRepository.SetPositionAsync(story.Id, temp);

        // 2) fecha buraco no Ã©pico origem (decrementa quem estava depois)
        await _storyRepository.DecrementPositionsAfterAsync(fromEpicId, fromPosition);

        // 3) abre espaÃ§o no Ã©pico destino (incrementa >= toPosition)
        await _storyRepository.IncrementPositionsFromAsync(toEpicId, toPosition);

        // 4) move de fato: muda EpicId e Position para o destino
        await _storyRepository.SetEpicAndPositionAsync(story.Id, toEpicId, toPosition);

        await tx.CommitAsync();
    }
    
    public async Task ArchiveEpicAsync(Guid projectId, int epicId, Guid userId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId, userId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
                      ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        var epic = await _epicRepository.GetByIdAsync(epicId)
                   ?? throw new NotFoundException("Ã‰pico não encontrado.");

        if (epic.ProductBacklogId != backlog.Id)
            throw new UnauthorizedAccessException("Este Ã©pico não pertence ao projeto informado.");

        await using var tx = await _context.Database.BeginTransactionAsync();

        await _epicRepository.ArchiveAsync(epicId);
        await _storyRepository.ArchiveByEpicIdAsync(epicId);

        await tx.CommitAsync();
    }

    public async Task RestoreEpicAsync(Guid projectId, int epicId, Guid userId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId, userId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
                      ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        var epic = await _epicRepository.GetByIdIncludingArchivedAsync(epicId)
                   ?? throw new NotFoundException("Ã‰pico não encontrado.");

        if (epic.ProductBacklogId != backlog.Id)
            throw new UnauthorizedAccessException("Este Ã©pico não pertence ao projeto informado.");

        await using var tx = await _context.Database.BeginTransactionAsync();

        await _epicRepository.RestoreAsync(epicId);
        await _storyRepository.RestoreByEpicIdAsync(epicId);

        await tx.CommitAsync();
    }
    
    public async Task ArchiveUserStoryAsync(Guid projectId, int storyId, Guid userId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId, userId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
                      ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        var story = await _storyRepository.GetByIdWithEpicAsync(storyId)
                    ?? throw new NotFoundException("User story não encontrada.");

        if (story.Epic.ProductBacklogId != backlog.Id)
            throw new UnauthorizedAccessException("Esta user story não pertence ao projeto informado.");

        await _storyRepository.ArchiveAsync(storyId);
    }

    public async Task RestoreUserStoryAsync(Guid projectId, int storyId, Guid userId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId, userId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var backlog = await _backlogRepository.GetByProjectIdAsync(projectId)
                      ?? throw new DomainException("Product backlog não encontrado para este projeto.");

        var story = await _storyRepository.GetByIdWithEpicIncludingArchivedAsync(storyId)
                    ?? throw new NotFoundException("User story não encontrada.");

        if (story.Epic.ProductBacklogId != backlog.Id)
            throw new UnauthorizedAccessException("Esta user story não pertence ao projeto informado.");

        await _storyRepository.RestoreAsync(storyId);
    }

}

