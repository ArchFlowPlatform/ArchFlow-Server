using archFlowServer.Models.Dtos.Sprint;
using archFlowServer.Models.Entities;
using archFlowServer.Models.Enums;
using archFlowServer.Models.Exceptions;
using archFlowServer.Repositories.Interfaces;

namespace archFlowServer.Services;

public class SprintService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ISprintRepository _sprintRepository;

    public SprintService(
        IProjectRepository projectRepository,
        ISprintRepository sprintRepository)
    {
        _projectRepository = projectRepository;
        _sprintRepository = sprintRepository;
    }

    // ==========================
    // Read
    // ==========================

    public async Task<IEnumerable<SprintResponseDto>> GetAllAsync(
        Guid projectId,
        bool includeArchived)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var sprints = await _sprintRepository.GetAllAsync(projectId, includeArchived);

        return sprints.Select(MapToDto);
    }

    public async Task<SprintResponseDto> GetByIdAsync(
        Guid projectId,
        Guid sprintId,
        bool includeArchived)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        Sprint? sprint;

        if (includeArchived)
        {
            sprint = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
                     ?? await _sprintRepository.GetArchivedByIdAsync(projectId, sprintId);
        }
        else
        {
            sprint = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId);
        }

        if (sprint is null)
            throw new NotFoundException("Sprint não encontrada.");

        return MapToDto(sprint);
    }

    // ==========================
    // Create / Update
    // ==========================

    public async Task<SprintResponseDto> CreateAsync(
        Guid projectId,
        CreateSprintDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        // validações "API-friendly" (o domínio valida de novo)
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException("Nome da sprint é obrigatório.");

        ValidateDates(dto.StartDate, dto.EndDate);

        if (dto.CapacityHours < 0)
            throw new ValidationException("CapacityHours não pode ser negativo.");

        var sprint = new Sprint(
            projectId: projectId,
            name: dto.Name,
            goal: dto.Goal,
            startDate: dto.StartDate,
            endDate: dto.EndDate,
            capacityHours: dto.CapacityHours
        );

        await _sprintRepository.AddAsync(sprint);

        // Board 1:1 por Sprint — criado junto
        var board = new Board(
            projectId: projectId,
            sprintId: sprint.Id,
            name: sprint.Name,
            description: dto.Goal,
            boardType: BoardType.Kanban
        );

        await _sprintRepository.AddBoardAsync(board);

        await _sprintRepository.SaveChangesAsync();

        return MapToDto(sprint);
    }

    public async Task<SprintResponseDto> UpdateAsync(
        Guid projectId,
        Guid sprintId,
        UpdateSprintDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var sprint = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException("Nome da sprint é obrigatório.");

        ValidateDates(dto.StartDate, dto.EndDate);

        if (dto.CapacityHours < 0)
            throw new ValidationException("CapacityHours não pode ser negativo.");

        // domínio já bloqueia, mas mantém mensagem consistente na service
        if (sprint.Status is SprintStatus.Closed or SprintStatus.Cancelled)
            throw new ConflictException("Não é permitido editar uma sprint fechada/cancelada.");

        sprint.UpdateDetails(
            name: dto.Name,
            goal: dto.Goal,
            startDate: dto.StartDate,
            endDate: dto.EndDate,
            capacityHours: dto.CapacityHours
        );

        await _sprintRepository.SaveChangesAsync();

        return MapToDto(sprint);
    }

    // ==========================
    // State transitions
    // ==========================

    public async Task ActivateAsync(Guid projectId, Guid sprintId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var sprint = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        // regra: 1 sprint Active por projeto
        var hasAnotherActive = await _sprintRepository.HasAnotherActiveSprintAsync(projectId, sprintId);
        if (hasAnotherActive)
            throw new ConflictException("Já existe uma sprint ativa neste projeto.");

        // domínio valida Planned -> Active
        sprint.Activate();

        await _sprintRepository.SaveChangesAsync();
    }

    public async Task CloseAsync(Guid projectId, Guid sprintId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var sprint = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        sprint.Close();

        await _sprintRepository.SaveChangesAsync();
    }

    public async Task CancelAsync(Guid projectId, Guid sprintId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var sprint = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        sprint.Cancel();

        await _sprintRepository.SaveChangesAsync();
    }

    // ==========================
    // Archive / Restore
    // ==========================

    public async Task ArchiveAsync(Guid projectId, Guid sprintId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var sprint = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        sprint.Archive();

        await _sprintRepository.SaveChangesAsync();
    }

    public async Task RestoreAsync(Guid projectId, Guid sprintId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var sprint = await _sprintRepository.GetArchivedByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint arquivada não encontrada.");

        sprint.Restore();

        await _sprintRepository.SaveChangesAsync();
    }

    // ==========================
    // Helpers
    // ==========================

    private static void ValidateDates(DateTime start, DateTime end)
    {
        if (start.Date >= end.Date)
            throw new ValidationException("StartDate deve ser menor que EndDate.");
    }

    private static SprintResponseDto MapToDto(Sprint sprint)
        => new(
            sprint.Id,
            sprint.ProjectId,
            sprint.Name,
            sprint.Goal,
            sprint.StartDate,
            sprint.EndDate,
            MapStatus(sprint.Status),
            sprint.CapacityHours,
            sprint.IsArchived,
            sprint.ArchivedAt,
            sprint.CreatedAt,
            sprint.UpdatedAt
        );

    private static SprintStatusDto MapStatus(SprintStatus status)
        => status switch
        {
            SprintStatus.Planned => SprintStatusDto.Planned,
            SprintStatus.Active => SprintStatusDto.Active,
            SprintStatus.Closed => SprintStatusDto.Closed,
            SprintStatus.Cancelled => SprintStatusDto.Cancelled,
            _ => SprintStatusDto.Planned
        };
}
