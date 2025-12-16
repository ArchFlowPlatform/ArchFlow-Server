using agileTrackerServer.Models.Entities;
using agileTrackerServer.Repositories.Interfaces;
using agileTrackerServer.Models.Dtos.Project;


namespace agileTrackerServer.Services
{
    public class ProjectService
    {
        private readonly IProjectRepository _repository;

        public ProjectService(IProjectRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ProjectResponseDto>> GetAllAsync()
        {
            var projects = await _repository.GetAllAsync();
            return projects.Select(MapToDto);
        }

        public async Task<ProjectResponseDto?> GetByIdAsync(Guid id, Guid OwnerId)
        {
            var project = await _repository.GetByIdAsync(id, OwnerId);
            return project is null ? null : MapToDto(project);
        }

        public async Task<ProjectResponseDto> CreateAsync(CreateProjectDto dto, Guid OwnerId)
        {
            
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                OwnerId = OwnerId,
                Status = "Active",          
                CreatedAt = DateTime.UtcNow,
            };

            await _repository.AddAsync(project);
            await _repository.SaveChangesAsync();

            // recarrega com Owner incluído se precisar do OwnerName
            var created = await _repository.GetByIdAsync(project.Id, OwnerId) ?? project;

            return MapToDto(created);
        }

        private static ProjectResponseDto MapToDto(Project p)
        {
            return new ProjectResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                OwnerId = p.OwnerId,
                OwnerName = p.Owner?.Name ?? string.Empty,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
            };
        }
    }
}
