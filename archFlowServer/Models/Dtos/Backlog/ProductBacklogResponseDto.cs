namespace archFlowServer.Models.Dtos.Backlog;

public class ProductBacklogResponseDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }

    public string Overview { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<EpicResponseDto> Epics { get; set; } = new();
}
