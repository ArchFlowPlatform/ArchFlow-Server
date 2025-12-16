using agileTrackerServer.Models.Dtos.User;
using agileTrackerServer.Models.Entities;
using agileTrackerServer.Models.Enums;
using agileTrackerServer.Repositories.Interfaces;
using agileTrackerServer.Utils;

namespace agileTrackerServer.Services
{
    public class UserService
    {
        private readonly IUserRepository _repository;
        private readonly PasswordHasher _hasher;
        private readonly TokenService _tokenService;

        public UserService(IUserRepository repository, PasswordHasher hasher, TokenService tokenService)
        {
            _repository = repository;
            _hasher = hasher;
            _tokenService = tokenService;
        }
        
        public async Task<ResponseUserDto?> GetByIdAsync(Guid id)
        {
            var user = await _repository.GetByIdAsync(id);
            return user is null ? null : MapToDto(user);
        }

        public async Task<(ResponseUserDto user, string token)> CreateAsync(CreateUserDto dto)
        {
            if (dto.Type == UserType.Admin)
                throw new Exception("Nâo é permitido criar um usuário admin!");
                
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

            // gerar token
            string token = _tokenService.GenerateToken(user);

            // buscar DTO completo
            var created = await _repository.GetByIdAsync(user.Id) ?? user;

            return (MapToDto(created), token);
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