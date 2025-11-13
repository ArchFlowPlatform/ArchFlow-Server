using agileTrackerServer.Models.Dtos.Project;
using agileTrackerServer.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace agileTrackerServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly ProjectService _service;

        public ProjectsController(ProjectService service)
        {
            _service = service;
        }

        // GET api/projects
        [HttpGet]
        [SwaggerOperation(Summary = "Lista todos os projetos.")]
        [ProducesResponseType(typeof(IEnumerable<ProjectResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProjectResponseDto>>> GetAll()
        {
            var projects = await _service.GetAllAsync();
            return Ok(projects);
        }

        // GET api/projects/{id}
        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "Busca um projeto pelo ID.")]
        [ProducesResponseType(typeof(ProjectResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectResponseDto>> GetById(Guid id)
        {
            var project = await _service.GetByIdAsync(id);
            if (project == null) return NotFound();
            return Ok(project);
        }

        // POST api/projects
        [HttpPost]
        [SwaggerOperation(Summary = "Cria um novo projeto.")]
        [ProducesResponseType(typeof(ProjectResponseDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<ProjectResponseDto>> Create([FromBody] CreateProjectDto request)
        {
            var project = await _service.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = project.Id },
                project
            );
        }
    }
}