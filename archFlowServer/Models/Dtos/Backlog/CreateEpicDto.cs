using archFlowServer.Models.Enums;

namespace archFlowServer.Models.Dtos.Backlog;

public class CreateEpicDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public BusinessValue BusinessValue { get; set; } = BusinessValue.Medium;
    public EpicStatus Status { get; set; } = EpicStatus.Draft;
    public int Priority { get; set; } = 0;
    public string? Color { get; set; }
    public bool IsArchived { get; set; } = false;
}
