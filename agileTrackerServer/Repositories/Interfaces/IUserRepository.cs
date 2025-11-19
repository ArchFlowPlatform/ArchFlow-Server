using agileTrackerServer.Models.Entities;

namespace agileTrackerServer.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(Guid id);
        Task AddAsync(User user);
        Task SaveChangesAsync();
        Task<bool> EmailExistsAsync(string email);
    }
}