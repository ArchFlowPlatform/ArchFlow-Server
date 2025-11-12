using agileTrackerServer.Models.Entities;
using agileTrackerServer.Services;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetAll()
        {
            var projects = await _service.GetAllAsync();
            return Ok(projects);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Project>> GetById(Guid id)
        {
            var project = await _service.GetByIdAsync(id);
            if (project == null) return NotFound();
            return Ok(project);
        }

        [HttpPost]
        public async Task<ActionResult<Project>> Create(Project request)
        {
            var project = await _service.CreateAsync(request.Name, request.Description, request.OwnerId);
            return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
        }
    }
}