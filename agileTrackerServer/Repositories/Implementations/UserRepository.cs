using agileTrackerServer.Data;
using agileTrackerServer.Models.Entities;
using agileTrackerServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace agileTrackerServer.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<IEnumerable<User>> GetAllAsync() =>
            await _context.Users.AsNoTracking().ToListAsync();
        
        public async Task<User?> GetByIdAsync(Guid id) =>
            await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        
        public async Task AddAsync(User user) =>
            await _context.Users.AddAsync(user);
        
        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();
        
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
        
    }
}