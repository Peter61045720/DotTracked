using System.Net;
using System.Net.Http.Json;
using DotTracked.Shared.DTOs;
using MudBlazor;

namespace DotTracked.Client.Services;

public class AbsenceService(HttpClient http, ISnackbar snackbar) : IAbsenceService
{
    public async Task<List<AbsenceDto>> GetAbsencesAsync()
    {
        var response = await http.GetAsync("api/absences");

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<AbsenceDto>>() ?? [];
    }

    public async Task<AbsenceDto?> GetAbsenceById(Guid id)
    {
        var response = await http.GetAsync($"api/absences/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The requested item was not found", Severity.Warning);
            return null;
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            snackbar.Add("You do not have permission to access this item", Severity.Error);
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AbsenceDto>();
    }

    public async Task<AbsenceDto> CreateAbsenceAsync(AbsenceDto absenceDto)
    {
        var response = await http.PostAsJsonAsync("api/absences", absenceDto);

        response.EnsureSuccessStatusCode();

        var createdAbsence = await response.Content.ReadFromJsonAsync<AbsenceDto>();

        return createdAbsence ?? throw new InvalidOperationException("The server did not return the created data");
    }

    public async Task UpdateAbsenceAsync(Guid id, AbsenceDto absenceDto)
    {
        var response = await http.PutAsJsonAsync($"api/absences/{id}", absenceDto);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The requested item was not found", Severity.Warning);
            return;
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            snackbar.Add("You do not have permission to update this item", Severity.Error);
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAbsenceAsync(Guid id)
    {
        var response = await http.DeleteAsync($"api/absences/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            snackbar.Add("The item no longer exists", Severity.Info);
            return;
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            snackbar.Add("You do not have permission to delete this item", Severity.Error);
            return;
        }

        response.EnsureSuccessStatusCode();
    }
}