using System.Security.Claims;
using DotTracked.Data;
using DotTracked.Data.Models;
using DotTracked.Shared.Constants;
using DotTracked.Shared.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DotTracked.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var adminGroup = app.MapGroup("/api/admin").RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

        var users = adminGroup.MapGroup("/users");

        users.MapGet("/", GetAllUsers);
        users.MapGet("/{id}", GetUserById);
        users.MapDelete("/{id}", DeleteUser);

        var groups = adminGroup.MapGroup("/groups");

        groups.MapGet("/", GetAllGroups);
        groups.MapGet("/{id:guid}", GetGroupById);
        groups.MapDelete("/{id:guid}", DeleteGroup);

        var members = groups.MapGroup("/{groupId:guid}/members");

        members.MapGet("/", GetAllGroupMembers);
        members.MapGet("/{userId}", GetGroupMemberById);
        members.MapPost("/{userId}", CreateGroupMember);
        members.MapPut("/{userId}/moderator", UpdateGroupModeratorStatus);
        members.MapDelete("/{userId}", DeleteGroupMember);
    }

    private static async Task<IResult> GetAllUsers(ApplicationDbContext db)
    {
        return TypedResults.Ok(await db.Users
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                Email = u.Email!,
                DisplayName = u.DisplayName,
                FirstName = u.FirstName,
                LastName = u.LastName,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .ToListAsync());
    }

    private static async Task<IResult> GetUserById(string id, ApplicationDbContext db)
    {
        var user = await db.Users
            .Where(u => u.Id == id)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                Email = u.Email!,
                DisplayName = u.DisplayName,
                FirstName = u.FirstName,
                LastName = u.LastName,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .SingleOrDefaultAsync();

        if (user is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(user);
    }

    private static async Task<IResult> DeleteUser(
        string id,
        ClaimsPrincipal user,
        UserManager<ApplicationUser> userManager)
    {
        var currentUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (id == currentUserId)
        {
            return TypedResults.BadRequest();
        }

        var userToDelete = await userManager.FindByIdAsync(id);

        if (userToDelete is null)
        {
            return TypedResults.NotFound();
        }

        var result = await userManager.DeleteAsync(userToDelete);

        if (!result.Succeeded)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.NoContent();
    }

    private static async Task<IResult> GetAllGroups(ApplicationDbContext db)
    {
        return TypedResults.Ok(await db.Groups
            .Select(g => new AdminGroupDto
            {
                Id = g.Id,
                Name = g.Name,
                MemberCount = g.Members.Count,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt
            })
            .ToListAsync());
    }

    private static async Task<IResult> GetGroupById(Guid id, ApplicationDbContext db)
    {
        var group = await db.Groups
            .Where(g => g.Id == id)
            .Select(g => new AdminGroupDto
            {
                Id = g.Id,
                Name = g.Name,
                MemberCount = g.Members.Count,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt
            })
            .SingleOrDefaultAsync();

        if (group is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(group);
    }

    private static async Task<IResult> DeleteGroup(Guid id, ApplicationDbContext db)
    {
        var groupToDelete = await db.Groups.FindAsync(id);

        if (groupToDelete is null)
        {
            return TypedResults.NotFound();
        }

        db.Groups.Remove(groupToDelete);
        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    private static async Task<IResult> GetAllGroupMembers(Guid groupId, ApplicationDbContext db)
    {
        return TypedResults.Ok(await db.GroupMembers
            .Where(gm => gm.GroupId == groupId)
            .Select(gm => new AdminGroupMemberDto
            {
                GroupId = gm.GroupId,
                UserId = gm.UserId,
                Email = gm.User.Email!,
                DisplayName = gm.User.DisplayName,
                FirstName = gm.User.FirstName,
                LastName = gm.User.LastName,
                IsModerator = gm.IsModerator,
                JoinedAt = gm.JoinedAt
            })
            .ToListAsync());
    }

    private static async Task<IResult> GetGroupMemberById(Guid groupId, string userId, ApplicationDbContext db)
    {
        var groupMember = await db.GroupMembers
            .Where(gm => gm.GroupId == groupId && gm.UserId == userId)
            .Select(gm => new AdminGroupMemberDto
            {
                GroupId = gm.GroupId,
                UserId = gm.UserId,
                Email = gm.User.Email!,
                DisplayName = gm.User.DisplayName,
                FirstName = gm.User.FirstName,
                LastName = gm.User.LastName,
                IsModerator = gm.IsModerator,
                JoinedAt = gm.JoinedAt
            })
            .SingleOrDefaultAsync();

        if (groupMember is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(groupMember);
    }

    private static async Task<IResult> CreateGroupMember(Guid groupId, string userId, ApplicationDbContext db)
    {
        var memberToAdd = await db.Users.FindAsync(userId);

        if (memberToAdd is null || !await db.Groups.AnyAsync(g => g.Id == groupId))
        {
            return TypedResults.NotFound();
        }

        var alreadyMember = await db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

        if (alreadyMember)
        {
            return TypedResults.BadRequest();
        }

        var groupMember = new GroupMember
        {
            GroupId = groupId,
            UserId = userId,
            IsModerator = false,
            JoinedAt = DateTime.UtcNow
        };

        await db.GroupMembers.AddAsync(groupMember);
        await db.SaveChangesAsync();

        return TypedResults.Created($"/groups/{groupId}/members/{userId}", new AdminUserDto
        {
            Id = memberToAdd.Id,
            Email = memberToAdd.Email!,
            DisplayName = memberToAdd.DisplayName,
            FirstName = memberToAdd.FirstName,
            LastName = memberToAdd.LastName,
            CreatedAt = memberToAdd.CreatedAt,
            UpdatedAt = memberToAdd.UpdatedAt
        });
    }

    private static async Task<IResult> UpdateGroupModeratorStatus(
        Guid groupId,
        string userId,
        bool isModerator,
        ApplicationDbContext db)
    {
        var groupMember = await db.GroupMembers.FindAsync(groupId, userId);

        if (groupMember is null)
        {
            return TypedResults.NotFound();
        }

        groupMember.IsModerator = isModerator;

        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    private static async Task<IResult> DeleteGroupMember(Guid groupId, string userId, ApplicationDbContext db)
    {
        var membershipToDelete = await db.GroupMembers.FindAsync(groupId, userId);

        if (membershipToDelete is null)
        {
            return TypedResults.NotFound();
        }

        if (membershipToDelete.IsModerator)
        {
            return TypedResults.BadRequest();
        }

        db.GroupMembers.Remove(membershipToDelete);
        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}