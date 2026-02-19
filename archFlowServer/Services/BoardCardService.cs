using archFlowServer.Data;
using archFlowServer.Models.Dtos.Board.Cards;
using archFlowServer.Models.Entities;
using archFlowServer.Models.Enums;
using archFlowServer.Models.Exceptions;
using archFlowServer.Repositories.Interfaces;
using ArchFlowServer.Models.Dtos.Board.Cards;

namespace archFlowServer.Services;

public class BoardCardService
{
    private readonly AppDbContext _context;
    private readonly IProjectRepository _projectRepository;
    private readonly ISprintRepository _sprintRepository;
    private readonly IBoardColumnRepository _columnRepository;
    private readonly IBoardCardRepository _cardRepository;
    private readonly IUserStoryRepository _userStoryRepository;

    public BoardCardService(
        AppDbContext context,
        IProjectRepository projectRepository,
        ISprintRepository sprintRepository,
        IBoardColumnRepository columnRepository,
        IBoardCardRepository cardRepository,
        IUserStoryRepository userStoryRepository)
    {
        _context = context;
        _projectRepository = projectRepository;
        _sprintRepository = sprintRepository;
        _columnRepository = columnRepository;
        _cardRepository = cardRepository;
        _userStoryRepository = userStoryRepository;
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

        EnsureSprintIsEditable(sprint);

        _ = await _columnRepository.GetByIdAsync(projectId, sprintId, columnId)
            ?? throw new NotFoundException("Coluna não encontrada.");

        _ = await EnsureUserStoryInProjectAsync(projectId, dto.UserStoryId);

        int position;
        if (dto.Position is null)
        {
            position = await _cardRepository.GetNextPositionAsync(columnId);
        }
        else
        {
            if (dto.Position.Value < 0) throw new ValidationException("Position inválida.");

            await _cardRepository.IncrementPositionsFromAsync(columnId, dto.Position.Value);
            position = dto.Position.Value;
        }

        var card = new BoardCard(columnId: columnId, userStoryId: dto.UserStoryId, position: position);

        await _cardRepository.AddAsync(card);
        await _cardRepository.SaveChangesAsync();

        return MapToDto(card);
    }

    // reorder dentro da mesma coluna
    public async Task ReorderAsync(Guid projectId, Guid sprintId, int columnId, ReorderBoardCardDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var sprint = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        EnsureSprintIsEditable(sprint);

        _ = await _columnRepository.GetByIdAsync(projectId, sprintId, columnId)
            ?? throw new NotFoundException("Coluna não encontrada.");

        if (dto.ToPosition < 0) throw new ValidationException("ToPosition inválida.");

        var card = await _cardRepository.GetByIdAsync(projectId, sprintId, dto.CardId)
            ?? throw new NotFoundException("Card não encontrado.");

        if (card.ColumnId != columnId)
            throw new ValidationException("Este card não pertence à coluna informada na rota.");

        var from = card.Position;

        var maxPos = await _cardRepository.GetMaxPositionAsync(columnId);
        var to = dto.ToPosition > maxPos ? maxPos : dto.ToPosition;

        if (from == to) return;

        var temp = maxPos + 1;

        await using var tx = await _context.Database.BeginTransactionAsync();

        await _cardRepository.SetPositionAsync(card.Id, temp);
        await _cardRepository.ShiftPositionsAsync(columnId, from, to);
        await _cardRepository.SetPositionAsync(card.Id, to);

        await tx.CommitAsync();
        await _cardRepository.SaveChangesAsync();
    }

    // move entre colunas (com ToPosition opcional)
    public async Task MoveAsync(Guid projectId, Guid sprintId, int cardId, MoveBoardCardDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var sprint = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        EnsureSprintIsEditable(sprint);

        var card = await _cardRepository.GetByIdAsync(projectId, sprintId, cardId)
            ?? throw new NotFoundException("Card não encontrado.");

        _ = await _columnRepository.GetByIdAsync(projectId, sprintId, dto.ToColumnId)
            ?? throw new NotFoundException("Coluna de destino não encontrada.");

        if (dto.ToPosition is < 0) throw new ValidationException("ToPosition inválida.");

        var fromColumnId = card.ColumnId;
        var fromPos = card.Position;

        await using var tx = await _context.Database.BeginTransactionAsync();

        // fecha buraco na origem
        await _cardRepository.DecrementPositionsAfterAsync(fromColumnId, fromPos);

        // define destino
        int toPos;
        if (dto.ToPosition is null)
        {
            toPos = await _cardRepository.GetNextPositionAsync(dto.ToColumnId);
        }
        else
        {
            // clamp no "final" permitido (max+1)
            var maxDest = await _cardRepository.GetMaxPositionAsync(dto.ToColumnId);
            var destCount = maxDest + 1;
            var clamped = dto.ToPosition.Value > destCount ? destCount : dto.ToPosition.Value;

            await _cardRepository.IncrementPositionsFromAsync(dto.ToColumnId, clamped);
            toPos = clamped;
        }

        await _cardRepository.SetColumnAsync(cardId, dto.ToColumnId);
        await _cardRepository.SetPositionAsync(cardId, toPos);

        await tx.CommitAsync();
        await _cardRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid projectId, Guid sprintId, int cardId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var sprint = await _sprintRepository.GetActiveByIdAsync(projectId, sprintId)
            ?? throw new NotFoundException("Sprint não encontrada.");

        EnsureSprintIsEditable(sprint);

        var card = await _cardRepository.GetByIdAsync(projectId, sprintId, cardId)
            ?? throw new NotFoundException("Card não encontrado.");

        var colId = card.ColumnId;
        var pos = card.Position;

        _cardRepository.Remove(card);
        await _cardRepository.SaveChangesAsync();

        await _cardRepository.DecrementPositionsAfterAsync(colId, pos);
        await _cardRepository.SaveChangesAsync();
    }

    private static BoardCardResponseDto MapToDto(BoardCard c)
        => new(c.Id, c.ColumnId, c.UserStoryId, c.Position, c.CreatedAt, c.UpdatedAt);

    private static void EnsureSprintIsEditable(Sprint sprint)
    {
        if (sprint.Status is SprintStatus.Closed or SprintStatus.Cancelled)
            throw new ConflictException("Não é possível alterar board de sprint fechada/cancelada.");
    }

    private async Task<UserStory> EnsureUserStoryInProjectAsync(Guid projectId, int storyId)
    {
        var story = await _userStoryRepository.GetByIdWithEpicAndBacklogAsync(storyId)
            ?? throw new NotFoundException("User story não encontrada.");

        if (story.Epic.ProductBacklog.ProjectId != projectId)
            throw new DomainException("Você não tem acesso a esta user story.");

        return story;
    }
}