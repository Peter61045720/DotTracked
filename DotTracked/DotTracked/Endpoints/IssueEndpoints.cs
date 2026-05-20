using System.Security.Claims;
using DotTracked.Data;
using DotTracked.Data.Models;
using DotTracked.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DotTracked.Endpoints;

public static class IssueEndpoints
{
    public static void MapIssueEndpoints(this IEndpointRouteBuilder app)
    {
        var issueGroup = app.MapGroup("/api/issues").RequireAuthorization();

        issueGroup.MapGet("/", async (ClaimsPrincipal user, ApplicationDbContext db) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var issues = await db.Issues
                .Where(issue => issue.CreatorId == userId && issue.GroupId == null)
                .Select(issue => new IssueDto
                {
                    Id = issue.Id,
                    Title = issue.Title,
                    Description = issue.Description,
                    Priority = issue.Priority,
                    Status = issue.Status,
                    StartDate = issue.StartDate,
                    DueDate = issue.DueDate,
                    EstimatedSeconds = issue.EstimatedSeconds
                })
                .ToListAsync();

            return Results.Ok(issues);
        });

        issueGroup.MapGet("/upcoming", async (ApplicationDbContext db, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var today = DateTime.Today;

            var issues = await db.Issues
                .Where(i => i.CreatorId == userId && i.GroupId == null && i.DueDate >= today)
                .OrderBy(i => i.DueDate)
                .Take(5)
                .Select(i => new IssueDto
                {
                    Id = i.Id,
                    Title = i.Title,
                    Description = i.Description,
                    Priority = i.Priority,
                    Status = i.Status,
                    StartDate = i.StartDate,
                    DueDate = i.DueDate,
                    EstimatedSeconds = i.EstimatedSeconds
                })
                .ToListAsync();

            return Results.Ok(issues);
        });

        issueGroup.MapGet("/{id:guid}", async (ClaimsPrincipal user, Guid id, ApplicationDbContext db) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var issue = await db.Issues.FindAsync(id);

            if (issue is null)
            {
                return Results.NotFound();
            }

            if (issue.CreatorId != userId || issue.GroupId is not null)
            {
                return Results.Forbid();
            }

            return Results.Ok(new IssueDto
            {
                Id = issue.Id,
                Title = issue.Title,
                Description = issue.Description,
                Priority = issue.Priority,
                Status = issue.Status,
                StartDate = issue.StartDate,
                DueDate = issue.DueDate,
                EstimatedSeconds = issue.EstimatedSeconds
            });
        });

        issueGroup.MapPost("/", async (ClaimsPrincipal user, ApplicationDbContext db, IssueDto issueDto) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var issue = new Issue
            {
                Title = issueDto.Title,
                Description = issueDto.Description,
                Priority = issueDto.Priority,
                Status = issueDto.Status,
                StartDate = issueDto.StartDate,
                DueDate = issueDto.DueDate,
                EstimatedSeconds = issueDto.EstimatedSeconds,
                CreatorId = userId
            };

            var currentTime = DateTime.UtcNow;
            issue.CreatedAt = currentTime;
            issue.UpdatedAt = currentTime;

            await db.Issues.AddAsync(issue);
            await db.SaveChangesAsync();

            issueDto.Id = issue.Id;

            return Results.Created($"/issue/{issueDto.Id}", issueDto);
        });

        issueGroup.MapPut("/{id:guid}",
            async (ClaimsPrincipal user, Guid id, ApplicationDbContext db, IssueDto issueDto) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId is null)
                {
                    return Results.Unauthorized();
                }

                var issue = await db.Issues.FindAsync(id);

                if (issue is null)
                {
                    return Results.NotFound();
                }

                if (issue.CreatorId != userId)
                {
                    return Results.Forbid();
                }

                issue.Title = issueDto.Title;
                issue.Description = issueDto.Description;
                issue.Priority = issueDto.Priority;
                issue.Status = issueDto.Status;
                issue.StartDate = issueDto.StartDate;
                issue.DueDate = issueDto.DueDate;
                issue.EstimatedSeconds = issueDto.EstimatedSeconds;
                issue.UpdatedAt = DateTime.UtcNow;

                await db.SaveChangesAsync();

                return Results.NoContent();
            });

        issueGroup.MapDelete("/{id:guid}", async (ClaimsPrincipal user, Guid id, ApplicationDbContext db) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var issue = await db.Issues.FindAsync(id);

            if (issue is null)
            {
                return Results.NotFound();
            }

            if (issue.CreatorId != userId)
            {
                return Results.Forbid();
            }

            db.Issues.Remove(issue);
            await db.SaveChangesAsync();

            return Results.Ok();
        });
    }
}