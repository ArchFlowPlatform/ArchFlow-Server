using archFlowServer.Models.Dtos.Labels;
using archFlowServer.Models.Entities;
using archFlowServer.Models.Exceptions;
using archFlowServer.Repositories.Interfaces;

namespace archFlowServer.Services;

public class LabelService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILabelRepository _labelRepository;

    public LabelService(IProjectRepository projectRepository, ILabelRepository labelRepository)
    {
        _projectRepository = projectRepository;
        _labelRepository = labelRepository;
    }

    public async Task<IEnumerable<LabelResponseDto>> GetAllAsync(Guid projectId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var labels = await _labelRepository.GetAllAsync(projectId);
        return labels.Select(MapToDto);
    }

    public async Task<LabelResponseDto> GetByIdAsync(Guid projectId, int labelId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var label = await _labelRepository.GetByIdAsync(projectId, labelId)
            ?? throw new NotFoundException("Label não encontrada.");

        return MapToDto(label);
    }

    public async Task<LabelResponseDto> CreateAsync(Guid projectId, CreateLabelDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var normalized = (dto.Name ?? string.Empty).Trim().ToLower();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ValidationException("Nome da label é obrigatório.");

        if (await _labelRepository.ExistsByNameAsync(projectId, normalized))
            throw new ConflictException("Já existe uma label com este nome.");

        var label = new Label(projectId, dto.Name, dto.Color);

        await _labelRepository.AddAsync(label);
        await _labelRepository.SaveChangesAsync();

        return MapToDto(label);
    }

    public async Task<LabelResponseDto> UpdateAsync(Guid projectId, int labelId, UpdateLabelDto dto)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var label = await _labelRepository.GetByIdAsync(projectId, labelId)
            ?? throw new NotFoundException("Label não encontrada.");

        var normalized = (dto.Name ?? string.Empty).Trim().ToLower();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ValidationException("Nome da label é obrigatório.");

        if (await _labelRepository.ExistsByNameAsync(projectId, normalized, excludeLabelId: labelId))
            throw new ConflictException("Já existe uma label com este nome.");

        label.Update(dto.Name, dto.Color);

        await _labelRepository.SaveChangesAsync();

        return MapToDto(label);
    }

    public async Task DeleteAsync(Guid projectId, int labelId)
    {
        _ = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var label = await _labelRepository.GetByIdAsync(projectId, labelId)
            ?? throw new NotFoundException("Label não encontrada.");

        _labelRepository.Remove(label);
        await _labelRepository.SaveChangesAsync();
    }

    private static LabelResponseDto MapToDto(Label l)
        => new(l.Id, l.ProjectId, l.Name, l.Color, l.CreatedAt, l.UpdatedAt);
}
