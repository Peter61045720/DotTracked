using DotTracked.Shared.DTOs;

namespace DotTracked.Client.Services;

public interface IAbsenceService
{
    Task<List<AbsenceDto>> GetAbsencesAsync();
    Task<AbsenceDto?> GetAbsenceById(Guid id);
    Task<AbsenceDto> CreateAbsenceAsync(AbsenceDto absenceDto);
    Task UpdateAbsenceAsync(Guid id, AbsenceDto absenceDto);
    Task DeleteAbsenceAsync(Guid id);
}