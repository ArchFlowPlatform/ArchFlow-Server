using archFlowServer.Models.Entities;
using ArchFlowServer.Models.Dtos.Project;

namespace archFlowServer.Repositories.Interfaces
{
    public interface IProjectRepository
    {
        // Projetos (repo neutro: sem userId/ownerId)
        Task<IReadOnlyList<Project>> GetAllActiveAsync();
        Task<Project?> GetActiveByIdAsync(Guid projectId);
        Task<Project?> GetArchivedByIdAsync(Guid projectId);

        // Opcional: se você precisar buscar independente de status
        Task<Project?> GetByIdAsync(Guid projectId);
        Task<IReadOnlyList<ProjectListDto>> GetWithDetailsAllActiveAsync();

        // Persistência
        Task AddAsync(Project project);
        Task SaveChangesAsync();

        // Members (dados; autorização fica fora)
        Task<IReadOnlyList<ProjectMember>> GetMembersAsync(Guid projectId);
    }
}