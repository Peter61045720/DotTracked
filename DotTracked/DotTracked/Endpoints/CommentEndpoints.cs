using System.Security.Claims;
using DotTracked.Data;
using DotTracked.Data.Models;
using DotTracked.Shared.Constants;
using DotTracked.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DotTracked.Endpoints;

public static class CommentEndpoints
{
    public static void MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var commentGroup = app.MapGroup("/api/issues/{issueId:guid}/comments")
            .RequireAuthorization(policy => policy.RequireRole(Roles.AppUser));

        commentGroup.MapGet("/", GetAllComments);
        commentGroup.MapGet("/count", GetCommentCount);
        commentGroup.MapGet("/{commentId:guid}", GetCommentById);
        commentGroup.MapPost("/", CreateComment);
        commentGroup.MapPut("/{commentId:guid}", UpdateComment);
        commentGroup.MapDelete("/{commentId:guid}", DeleteComment);
    }

    public static async Task<IResult> GetAllComments(Guid issueId, ApplicationDbContext db, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var comments = await db.Comments
            .Where(c => c.IssueId == issueId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                CreatorEmail = c.User.Email!,
                AvatarText = $"{char.ToUpper(c.User.FirstName[0])}{char.ToUpper(c.User.LastName[0])}",
                IsOwner = c.UserId == userId,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        return TypedResults.Ok(comments);
    }

    public static async Task<IResult> GetCommentCount(Guid issueId, ApplicationDbContext db, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var count = await db.Comments
            .Where(c => c.IssueId == issueId)
            .CountAsync();

        return TypedResults.Ok(count);
    }

    public static async Task<IResult> GetCommentById(
        Guid issueId,
        Guid commentId,
        ApplicationDbContext db,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var comment = await db.Comments
            .Where(c => c.Id == commentId)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                CreatorEmail = c.User.Email!,
                AvatarText = $"{char.ToUpper(c.User.FirstName[0])}{char.ToUpper(c.User.LastName[0])}",
                IsOwner = c.UserId == userId,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .SingleOrDefaultAsync();

        if (comment is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(comment);
    }

    public static async Task<IResult> CreateComment(
        Guid issueId,
        CommentDto commentDto,
        ApplicationDbContext db,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var appUser = await db.Users.FindAsync(userId);

        if (appUser is null)
        {
            return TypedResults.Unauthorized();
        }

        var currentTime = DateTime.UtcNow;

        var comment = new Comment
        {
            Content = commentDto.Content,
            CreatedAt = currentTime,
            UpdatedAt = currentTime,
            UserId = userId,
            IssueId = issueId
        };

        await db.Comments.AddAsync(comment);
        await db.SaveChangesAsync();

        commentDto.Id = comment.Id;
        commentDto.CreatorEmail = appUser.Email!;
        commentDto.AvatarText = $"{char.ToUpper(appUser.FirstName[0])}{char.ToUpper(appUser.LastName[0])}";
        commentDto.IsOwner = true;
        commentDto.CreatedAt = currentTime;
        commentDto.UpdatedAt = currentTime;

        return TypedResults.Created($"/comments/{commentDto.Id}", commentDto);
    }

    public static async Task<IResult> UpdateComment(
        Guid issueId,
        Guid commentId,
        CommentDto commentDto,
        ApplicationDbContext db,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var comment = await db.Comments.SingleOrDefaultAsync(c =>
            c.Id == commentId && c.UserId == userId && c.IssueId == issueId);

        if (comment is null)
        {
            return TypedResults.NotFound();
        }

        comment.Content = commentDto.Content;
        comment.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    public static async Task<IResult> DeleteComment(
        Guid issueId,
        Guid commentId,
        ApplicationDbContext db,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var comment = await db.Comments.SingleOrDefaultAsync(c =>
            c.Id == commentId && c.UserId == userId && c.IssueId == issueId);

        if (comment is null)
        {
            return TypedResults.NotFound();
        }

        db.Comments.Remove(comment);
        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}