using agileTrackerServer.Models.Entities;
using agileTrackerServer.Repositories.Interfaces;

namespace agileTrackerServer.Services
{
    public class ProjectService
    {
        private readonly IProjectRepository _repo;

        public ProjectService(IProjectRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<Project>> GetAllAsync() =>
            await _repo.GetAllAsync();

        public async Task<Project?> GetByIdAsync(Guid id) =>
            await _repo.GetByIdAsync(id);

        public async Task<Project> CreateAsync(string name, string description, Guid ownerId)
        {
            var project = new Project
            {
                Name = name,
                Description = description,
                OwnerId = ownerId
            };

            await _repo.AddAsync(project);
            await _repo.SaveChangesAsync();
            return project;
        }
    }
}