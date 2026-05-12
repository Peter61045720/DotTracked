using System.Security.Claims;
using DotTracked.Data;
using DotTracked.Data.Models;
using DotTracked.Shared.Constants;
using DotTracked.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DotTracked.Endpoints;

public static class WorkLogEndpoints
{
    public static void MapWorkLogEndpoints(this IEndpointRouteBuilder app)
    {
        var userWorkLogGroup = app.MapGroup("/api/worklogs")
            .RequireAuthorization(policy => policy.RequireRole(Roles.AppUser));

        userWorkLogGroup.MapGet("/", GetAllUserWorkLogs);

        var workLogGroup = app.MapGroup("/api/issues/{issueId:guid}/worklogs")
            .RequireAuthorization(policy => policy.RequireRole(Roles.AppUser));

        workLogGroup.MapGet("/", GetAllWorkLogs);
        workLogGroup.MapGet("/total", GetTotalLoggedTime);
        workLogGroup.MapGet("/{workLogId:guid}", GetWorkLogById);
        workLogGroup.MapPost("/", CreateWorkLog);
        workLogGroup.MapPut("/{workLogId:guid}", UpdateWorkLog);
        workLogGroup.MapDelete("/{workLogId:guid}", DeleteWorkLog);
    }

    public static async Task<IResult> GetAllUserWorkLogs(ApplicationDbContext db, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var workLogs = await db.WorkLogs
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.DateOfLogging)
            .Select(w => new WorkLogDto
            {
                Id = w.Id,
                Description = w.Description,
                TimeSpentSeconds = w.TimeSpentSeconds,
                DateOfLogging = w.DateOfLogging,
                IsOwner = true,
                CreatedAt = w.CreatedAt,
                UpdatedAt = w.UpdatedAt
            })
            .ToListAsync();

        return TypedResults.Ok(workLogs);
    }

    public static async Task<IResult> GetAllWorkLogs(Guid issueId, ApplicationDbContext db, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var workLogs = await db.WorkLogs
            .Where(w => w.IssueId == issueId)
            .OrderByDescending(w => w.DateOfLogging)
            .Select(w => new WorkLogDto
            {
                Id = w.Id,
                Description = w.Description,
                TimeSpentSeconds = w.TimeSpentSeconds,
                DateOfLogging = w.DateOfLogging,
                IsOwner = w.UserId == userId,
                CreatedAt = w.CreatedAt,
                UpdatedAt = w.UpdatedAt
            })
            .ToListAsync();

        return TypedResults.Ok(workLogs);
    }

    public static async Task<IResult> GetTotalLoggedTime(Guid issueId, ApplicationDbContext db, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var totalSeconds = await db.WorkLogs
            .Where(w => w.IssueId == issueId)
            .SumAsync(w => w.TimeSpentSeconds);

        return TypedResults.Ok(totalSeconds);
    }

    public static async Task<IResult> GetWorkLogById(
        Guid issueId,
        Guid workLogId,
        ApplicationDbContext db,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var workLog = await db.WorkLogs
            .Where(w => w.Id == workLogId)
            .Select(w => new WorkLogDto
            {
                Id = w.Id,
                Description = w.Description,
                TimeSpentSeconds = w.TimeSpentSeconds,
                DateOfLogging = w.DateOfLogging,
                IsOwner = w.UserId == userId,
                CreatedAt = w.CreatedAt,
                UpdatedAt = w.UpdatedAt
            })
            .SingleOrDefaultAsync();

        if (workLog is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(workLog);
    }

    public static async Task<IResult> CreateWorkLog(
        Guid issueId,
        WorkLogDto workLogDto,
        ApplicationDbContext db,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var currentTime = DateTime.UtcNow;

        var workLog = new WorkLog
        {
            Description = workLogDto.Description,
            TimeSpentSeconds = workLogDto.TimeSpentSeconds,
            DateOfLogging = workLogDto.DateOfLogging,
            CreatedAt = currentTime,
            UpdatedAt = currentTime,
            UserId = userId,
            IssueId = issueId
        };

        await db.WorkLogs.AddAsync(workLog);
        await db.SaveChangesAsync();

        workLogDto.Id = workLog.Id;

        return TypedResults.Created($"/worklogs/{workLogDto.Id}", workLogDto);
    }

    public static async Task<IResult> UpdateWorkLog(
        Guid issueId,
        Guid workLogId,
        WorkLogDto workLogDto,
        ApplicationDbContext db,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var workLog = await db.WorkLogs.SingleOrDefaultAsync(w =>
            w.Id == workLogId && w.UserId == userId && w.IssueId == issueId);

        if (workLog is null)
        {
            return TypedResults.NotFound();
        }

        workLog.Description = workLogDto.Description;
        workLog.TimeSpentSeconds = workLogDto.TimeSpentSeconds;
        workLog.DateOfLogging = workLogDto.DateOfLogging;
        workLog.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    public static async Task<IResult> DeleteWorkLog(
        Guid issueId,
        Guid workLogId,
        ApplicationDbContext db,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var workLog = await db.WorkLogs.SingleOrDefaultAsync(w =>
            w.Id == workLogId && w.UserId == userId && w.IssueId == issueId);

        if (workLog is null)
        {
            return TypedResults.NotFound();
        }

        db.WorkLogs.Remove(workLog);
        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}