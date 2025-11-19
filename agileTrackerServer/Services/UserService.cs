using agileTrackerServer.Models.Dtos.User;
using agileTrackerServer.Models.Entities;
using agileTrackerServer.Repositories.Interfaces;
using agileTrackerServer.Utils;

namespace agileTrackerServer.Services
{
    public class UserService
    {
        private readonly IUserRepository _repository;
        private readonly PasswordHasher _hasher;

        public UserService(IUserRepository repository, PasswordHasher hasher)
        {
            _repository = repository;
            _hasher = hasher;
        }
        
        public async Task<ResponseUserDto?> GetByIdAsync(Guid id)
        {
            var user = await _repository.GetByIdAsync(id);
            return user is null ? null : MapToDto(user);
        }

        public async Task<ResponseUserDto> CreateAsync(CreateUserDto dto)
        {
            if (await _repository.EmailExistsAsync(dto.Email))
                throw new Exception("Já existe um usuário com este email.");
            
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                Type = dto.Type,
                PasswordHash = _hasher.HashPassword(dto.Password),
                AvatarUrl = dto.AvatarUrl
            };
        
            await _repository.AddAsync(user);
            await _repository.SaveChangesAsync();

            var created = await _repository.GetByIdAsync(user.Id) ?? user;
            
            return MapToDto(created);
        }
        
        private static ResponseUserDto MapToDto(User u)
        {
            return new ResponseUserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Type = u.Type,
                AvatarUrl = u.AvatarUrl,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            };
        }
    }
}