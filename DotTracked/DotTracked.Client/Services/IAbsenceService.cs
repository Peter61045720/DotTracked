using DotTracked.Shared.DTOs;

namespace DotTracked.Client.Services;

public interface IAbsenceService
{
    Task<List<AbsenceDto>> GetAbsencesAsync();
    Task<List<AbsenceDto>> GetUpcomingAbsencesAsync();
    Task<AbsenceDto?> GetAbsenceByIdAsync(Guid id);
    Task<AbsenceDto> CreateAbsenceAsync(AbsenceDto absenceDto);
    Task UpdateAbsenceAsync(Guid id, AbsenceDto absenceDto);
    Task DeleteAbsenceAsync(Guid id);
}