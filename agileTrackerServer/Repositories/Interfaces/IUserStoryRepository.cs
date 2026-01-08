using agileTrackerServer.Models.Entities;

namespace agileTrackerServer.Repositories.Interfaces;

public interface IUserStoryRepository
{
    Task<UserStory?> GetByIdAsync(Guid storyId);
    Task AddAsync(UserStory story);
    Task SaveChangesAsync();
}