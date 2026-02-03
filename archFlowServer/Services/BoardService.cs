using archFlowServer.Models.Dtos.Board;
using archFlowServer.Models.Enums;
using archFlowServer.Models.Exceptions;
using archFlowServer.Repositories.Interfaces;

namespace archFlowServer.Services;

public class BoardService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ISprintRepository _sprintRepository;
    private readonly IBoardRepository _boardRepository;

    public BoardService(
        IProjectRepository projectRepository,
        ISprintRepository sprintRepository,
        IBoardRepository boardRepository)
    {
        _projectRepository = projectRepository;
        _sprintRepository = sprintRepository;
        _boardRepository = boardRepository;
    }

    public async Task<BoardResponseDto> GetBySprintAsync(Guid projectId, Guid sprintId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        // garante sprint existe e está ativa (não arquivada)
        _ = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        var board = await _boardRepository.GetBySprintIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Board não encontrado para esta sprint.");

        return MapToDto(board);
    }

    public async Task<BoardResponseDto> UpdateAsync(Guid projectId, Guid sprintId, UpdateBoardDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var sprint = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        if (sprint.Status is SprintStatus.Closed or SprintStatus.Cancelled)
            throw new ConflictException("Não é possível alterar o board de uma sprint fechada/cancelada.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException("Nome do board é obrigatório.");

        var board = await _boardRepository.GetBySprintIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Board não encontrado para esta sprint.");

        board.Update(dto.Name, dto.Description, dto.BoardType);

        await _boardRepository.SaveChangesAsync();

        return MapToDto(board);
    }

    private static BoardResponseDto MapToDto(Models.Entities.Board board)
        => new(
            board.Id,
            board.ProjectId,
            board.SprintId,
            board.Name,
            board.Description,
            board.BoardType,
            board.CreatedAt,
            board.UpdatedAt
        );
}
