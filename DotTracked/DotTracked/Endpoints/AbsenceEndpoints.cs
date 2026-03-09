using System.Security.Claims;
using DotTracked.Data;
using DotTracked.Data.Models;
using DotTracked.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DotTracked.Endpoints;

public static class AbsenceEndpoints
{
    public static void MapAbsenceEndpoints(this IEndpointRouteBuilder app)
    {
        var absenceGroup = app.MapGroup("/api/absences").RequireAuthorization();

        absenceGroup.MapGet("/", async (ClaimsPrincipal user, ApplicationDbContext db) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var absences = await db.Absences
                .Where(absence => absence.UserId == userId)
                .Select(absence => new AbsenceDto
                {
                    Id = absence.Id,
                    Type = absence.Type,
                    Description = absence.Description,
                    StartDate = absence.StartDate,
                    EndDate = absence.EndDate
                })
                .ToListAsync();

            return Results.Ok(absences);
        });

        absenceGroup.MapGet("/{id:guid}", async (ClaimsPrincipal user, Guid id, ApplicationDbContext db) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var absence = await db.Absences.FindAsync(id);

            if (absence is null)
            {
                return Results.NotFound();
            }

            if (absence.UserId != userId)
            {
                return Results.Forbid();
            }

            return Results.Ok(new AbsenceDto
            {
                Id = absence.Id,
                Type = absence.Type,
                Description = absence.Description,
                StartDate = absence.StartDate,
                EndDate = absence.EndDate
            });
        });

        absenceGroup.MapPost("/", async (ClaimsPrincipal user, ApplicationDbContext db, AbsenceDto absenceDto) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var currentTime = DateTime.UtcNow;

            var absence = new Absence
            {
                Type = absenceDto.Type,
                Description = absenceDto.Description,
                StartDate = absenceDto.StartDate,
                EndDate = absenceDto.EndDate,
                CreatedAt = currentTime,
                UpdatedAt = currentTime,
                UserId = userId
            };

            await db.Absences.AddAsync(absence);
            await db.SaveChangesAsync();

            absenceDto.Id = absence.Id;

            return Results.Created($"/absences/{absenceDto.Id}", absenceDto);
        });

        absenceGroup.MapPut("/{id:guid}",
            async (ClaimsPrincipal user, Guid id, ApplicationDbContext db, AbsenceDto absenceDto) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId is null)
                {
                    return Results.Unauthorized();
                }

                var absence = await db.Absences.FindAsync(id);

                if (absence is null)
                {
                    return Results.NotFound();
                }

                if (absence.UserId != userId)
                {
                    return Results.Forbid();
                }

                absence.Type = absenceDto.Type;
                absence.Description = absenceDto.Description;
                absence.StartDate = absenceDto.StartDate;
                absence.EndDate = absenceDto.EndDate;

                await db.SaveChangesAsync();

                return Results.NoContent();
            });

        absenceGroup.MapDelete("/{id:guid}", async (ClaimsPrincipal user, Guid id, ApplicationDbContext db) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var absence = await db.Absences.FindAsync(id);

            if (absence is null)
            {
                return Results.NotFound();
            }

            if (absence.UserId != userId)
            {
                return Results.Forbid();
            }

            db.Absences.Remove(absence);
            await db.SaveChangesAsync();

            return Results.Ok();
        });
    }
}