using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Models.Dtos.Backlog;

public class UpdateBacklogOverviewDto
{
    [SwaggerSchema("Overview do projeto serÃ¡ alterado.")]
    [Required(ErrorMessage = "Overview Ã© obrigatÃ³rio.")]
    public string? Overview { get; set; }
}
