using agileTrackerServer.Data;
using agileTrackerServer.Models.Entities;
using agileTrackerServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace agileTrackerServer.Repositories.Implementations;

public class UserStoryRepository : IUserStoryRepository
{
    private readonly AppDbContext _context;

    public UserStoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserStory?> GetByIdAsync(Guid storyId)
        => await _context.UserStories.FirstOrDefaultAsync(us => us.Id == storyId);

    public async Task AddAsync(UserStory story)
        => await _context.UserStories.AddAsync(story);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}