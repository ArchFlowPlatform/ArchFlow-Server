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

        // Exemplo: método placeholder
        public string HealthCheck() => "ProjectService OK";
    }
}