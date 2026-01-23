using archFlowServer.Models.Dtos.User;
using archFlowServer.Models.Entities;
using archFlowServer.Models.Enums;
using archFlowServer.Models.Exceptions;
using archFlowServer.Repositories.Interfaces;
using archFlowServer.Utils;

namespace archFlowServer.Services;

public class UserService
{
    private readonly IUserRepository _repository;
    private readonly PasswordHasher _hasher;
    private readonly TokenService _tokenService;

    public UserService(
        IUserRepository repository,
        PasswordHasher hasher,
        TokenService tokenService)
    {
        _repository = repository;
        _hasher = hasher;
        _tokenService = tokenService;
    }

    // ============================
    // GET BY ID
    // ============================
    public async Task<ResponseUserDto> GetByIdAsync(Guid id)
    {
        var user = await _repository.GetByIdAsync(id)
            ?? throw new DomainException("usuário não encontrado.");

        return MapToDto(user);
    }
    
    public async Task<ResponseUserDto> GetByEmailAsync(string email)
    {
        var user = await _repository.GetByEmailAsync(email)
                   ?? throw new NotFoundException("usuário não encontrado.");

        return MapToDto(user);
    }

    // ============================
    // CREATE
    // ============================
    public async Task<(ResponseUserDto user, string token)> CreateAsync(CreateUserDto dto)
    {
        if (dto.Type == UserType.Admin)
            throw new ForbiddenException("não Ã© permitido criar um usuário administrador.");

        if (await _repository.EmailExistsAsync(dto.Email))
            throw new ConflictException("JÃ¡ existe um usuário com este email.");

        var user = new User(
            dto.Name,
            dto.Email,
            dto.Type,
            _hasher.HashPassword(dto.Password),
            dto.AvatarUrl
        );
        
        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);

        return (MapToDto(user), token);
    }

    // ============================
    // MAP
    // ============================
    private static ResponseUserDto MapToDto(User user)
    {
        return new ResponseUserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Type = user.Type,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}

