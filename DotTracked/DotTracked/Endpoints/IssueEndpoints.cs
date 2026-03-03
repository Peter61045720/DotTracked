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
                .Where(issue => issue.CreatorId == userId)
                .Select(issue => new IssueDto
                {
                    Id = issue.Id,
                    Title = issue.Title,
                    Description = issue.Description,
                    Priority = issue.Priority,
                    Status = issue.Status,
                    DueDate = issue.DueDate
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

            if (issue.CreatorId != userId)
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
                DueDate = issue.DueDate
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
                DueDate = issueDto.DueDate,
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
                issue.DueDate = issueDto.DueDate;
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