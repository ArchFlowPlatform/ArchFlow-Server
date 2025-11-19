using agileTrackerServer.Models.Dtos.Project;
using agileTrackerServer.Models.Dtos.User;
using agileTrackerServer.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace agileTrackerServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _service;

    public UsersController(UserService service)
    {
        _service = service;
    }
    
    // GET api/users/{id}
    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Busca um usuário pelo ID.")]
    [ProducesResponseType(typeof(ResponseUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResponseUserDto>> GetById(Guid id)
    {
        var project = await _service.GetByIdAsync(id);
        if (project == null) return NotFound();
        return Ok(project);
    }
    
    // POST api/users
    [HttpPost]
    [SwaggerOperation(Summary = "Cria um novo usuário.")]
    [ProducesResponseType(typeof(ResponseUserDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ResponseUserDto>> Create([FromBody] CreateUserDto request)
    {
        try
        {
            var user = await _service.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = user.Id },
                user
            );
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

}