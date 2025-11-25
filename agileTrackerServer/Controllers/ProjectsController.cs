using agileTrackerServer.Models.Dtos.Project;
using agileTrackerServer.Models.ViewModels;
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
        [ProducesResponseType(typeof(ResultViewModel<IEnumerable<ProjectResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var projects = await _service.GetAllAsync();

            return Ok(
                ResultViewModel<IEnumerable<ProjectResponseDto>>.Ok(
                    "Lista de projetos obtida com sucesso!",
                    projects
                )
            );
        }

        // GET api/projects/{id}
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Busca um projeto pelo ID.")]
        [ProducesResponseType(typeof(ResultViewModel<ProjectResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id)
        {
            // 1. Validar GUID manualmente
            if (!Guid.TryParse(id, out var guid))
            {
                return BadRequest(
                    ResultViewModel.Fail(
                        "O ID informado é inválido.",
                        new List<string> { "O parâmetro não é um GUID válido." }
                    )
                );
            }

            // 2. Buscar o projeto
            var project = await _service.GetByIdAsync(guid);

            if (project == null)
            {
                return NotFound(
                    ResultViewModel.Fail(
                        "Projeto não encontrado!",
                        new List<string> { "Nenhum projeto com esse ID foi localizado." }
                    )
                );
            }

            // 3. Retorno padrão
            return Ok(
                ResultViewModel<ProjectResponseDto>.Ok(
                    "Projeto encontrado com sucesso!",
                    project
                )
            );
        }

        // POST api/projects
        [HttpPost]
        [SwaggerOperation(Summary = "Cria um novo projeto.")]
        [ProducesResponseType(typeof(ResultViewModel<ProjectResponseDto>), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateProjectDto request)
        {
            var project = await _service.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = project.Id },
                ResultViewModel<ProjectResponseDto>.Ok(
                    "Projeto criado com sucesso!",
                    project
                )
            );
        }
    }
}
