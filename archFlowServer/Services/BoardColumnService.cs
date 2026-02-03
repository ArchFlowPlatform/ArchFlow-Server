using archFlowServer.Models.Entities;
using archFlowServer.Models.Enums;
using archFlowServer.Models.Exceptions;
using archFlowServer.Repositories.Interfaces;
using ArchFlowServer.Models.Dtos.Board.Columns;

namespace archFlowServer.Services;

public class BoardColumnService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ISprintRepository _sprintRepository;
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardColumnRepository _columnRepository;

    public BoardColumnService(
        IProjectRepository projectRepository,
        ISprintRepository sprintRepository,
        IBoardRepository boardRepository,
        IBoardColumnRepository columnRepository)
    {
        _projectRepository = projectRepository;
        _sprintRepository = sprintRepository;
        _boardRepository = boardRepository;
        _columnRepository = columnRepository;
    }

    public async Task<IEnumerable<BoardColumnResponseDto>> GetAllAsync(Guid projectId, Guid sprintId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        _ = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        var columns = await _columnRepository.GetAllAsync(projectId, sprintId);
        return columns.Select(MapToDto);
    }

    public async Task<BoardColumnResponseDto> CreateAsync(Guid projectId, Guid sprintId, CreateBoardColumnDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var sprint = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        if (sprint.Status is SprintStatus.Closed or SprintStatus.Cancelled)
            throw new ConflictException("Não é possível alterar board de sprint fechada/cancelada.");

        var board = await _boardRepository.GetBySprintIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Board não encontrado para esta sprint.");

        int position;
        if (dto.Position is null)
        {
            position = await _columnRepository.GetNextPositionAsync(board.Id);
        }
        else
        {
            if (dto.Position.Value < 0)
                throw new ValidationException("Position inválida.");

            await _columnRepository.IncrementPositionsFromAsync(board.Id, dto.Position.Value);
            position = dto.Position.Value;
        }

        var column = new BoardColumn(
            boardId: board.Id,
            name: dto.Name,
            description: dto.Description,
            position: position,
            wipLimit: dto.WipLimit,
            color: dto.Color,
            isDoneColumn: dto.IsDoneColumn
        );

        await _columnRepository.AddAsync(column);
        await _columnRepository.SaveChangesAsync();

        return MapToDto(column);
    }

    public async Task<BoardColumnResponseDto> UpdateAsync(Guid projectId, Guid sprintId, int columnId, UpdateBoardColumnDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var sprint = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        if (sprint.Status is SprintStatus.Closed or SprintStatus.Cancelled)
            throw new ConflictException("Não é possível alterar board de sprint fechada/cancelada.");

        var column = await _columnRepository.GetByIdAsync(projectId, sprintId, columnId)
            ?? throw new NotFoundException("Coluna não encontrada.");

        column.Update(dto.Name, dto.Description, dto.WipLimit, dto.Color, dto.IsDoneColumn);

        await _columnRepository.SaveChangesAsync();
        return MapToDto(column);
    }

    public async Task DeleteAsync(Guid projectId, Guid sprintId, int columnId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var sprint = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        if (sprint.Status is SprintStatus.Closed or SprintStatus.Cancelled)
            throw new ConflictException("Não é possível alterar board de sprint fechada/cancelada.");

        var column = await _columnRepository.GetByIdAsync(projectId, sprintId, columnId)
            ?? throw new NotFoundException("Coluna não encontrada.");

        var removedPos = column.Position;

        _columnRepository.Remove(column);
        await _columnRepository.SaveChangesAsync();

        // fecha buraco
        await _columnRepository.DecrementPositionsAfterAsync(column.BoardId, removedPos);
    }

    private static BoardColumnResponseDto MapToDto(BoardColumn c)
        => new(
            c.Id, c.BoardId, c.Name, c.Description, c.Position,
            c.WipLimit, c.Color, c.IsDoneColumn,
            c.CreatedAt, c.UpdatedAt
        );
}
