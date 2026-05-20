using System.Security.Claims;
using DotTracked.Data;
using DotTracked.Data.Models;
using DotTracked.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DotTracked.Endpoints;

public static class GroupEndpoints
{
    public static void MapGroupEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/groups").RequireAuthorization();

        group.MapGet("/", GetAllGroups);
        group.MapGet("/preview", GetGroupsPreview);
        group.MapGet("/{id:guid}", GetGroupById);
        group.MapPost("/", CreateGroup);
        group.MapPut("/{id:guid}", UpdateGroup);
        group.MapDelete("/{id:guid}", DeleteGroup);

        var members = group.MapGroup("/{groupId:guid}/members");

        members.MapGet("/", GetAllGroupMembers);
        members.MapGet("/details", GetGroupMemberByEmail);
        members.MapPost("/", CreateGroupMember);
        members.MapDelete("/", DeleteGroupMember);
        members.MapDelete("/leave", LeaveGroup);

        var issues = group.MapGroup("/{groupId:guid}/issues");

        issues.MapGet("/", GetAllGroupIssues);
        issues.MapGet("/{issueId:guid}", GetGroupIssueById);
        issues.MapPost("/", CreateGroupIssue);
        issues.MapPut("/{issueId:guid}", UpdateGroupIssue);
        issues.MapDelete("/{issueId:guid}", DeleteGroupIssue);

        var assignments = issues.MapGroup("/{issueId:guid}/assignments");

        assignments.MapGet("/", GetIssueAssignees);
        assignments.MapPost("/", CreateAssignment);
        assignments.MapDelete("/", DeleteAssignment);
    }

    private static async Task<IResult> GetAllGroups(ApplicationDbContext db, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var groups = await db.GroupMembers
            .Where(gm => gm.UserId == userId)
            .Select(gm => new GroupDto
            {
                Id = gm.GroupId,
                Name = gm.Group.Name,
                IsModerator = gm.IsModerator,
                JoinedAt = gm.JoinedAt,
                MemberCount = gm.Group.Members.Count
            })
            .ToListAsync();

        return TypedResults.Ok(groups);
    }

    private static async Task<IResult> GetGroupsPreview(ApplicationDbContext db, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var groups = await db.GroupMembers
            .Where(gm => gm.UserId == userId)
            .OrderBy(gm => gm.Group.Name)
            .Take(5)
            .Select(gm => new GroupNameDto
            {
                Id = gm.GroupId,
                Name = gm.Group.Name
            })
            .ToListAsync();

        return TypedResults.Ok(groups);
    }

    private static async Task<IResult> GetGroupById(Guid id, ApplicationDbContext db, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var group = await db.GroupMembers
            .Where(gm => gm.GroupId == id && gm.UserId == userId)
            .Select(gm => new GroupDto
            {
                Id = gm.GroupId,
                Name = gm.Group.Name,
                IsModerator = gm.IsModerator,
                JoinedAt = gm.JoinedAt,
                MemberCount = gm.Group.Members.Count
            })
            .SingleOrDefaultAsync();

        if (group is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(group);
    }

    private static async Task<IResult> CreateGroup(GroupDto groupDto, ApplicationDbContext db, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var currentTime = DateTime.UtcNow;

        var group = new Group
        {
            Name = groupDto.Name,
            CreatedAt = currentTime,
            UpdatedAt = currentTime
        };

        var groupMember = new GroupMember
        {
            Group = group,
            UserId = userId,
            IsModerator = true,
            JoinedAt = currentTime
        };

        await db.GroupMembers.AddAsync(groupMember);
        await db.SaveChangesAsync();

        groupDto.Id = group.Id;
        groupDto.IsModerator = true;
        groupDto.JoinedAt = currentTime;
        groupDto.MemberCount = 1;

        return TypedResults.Created($"/groups/{groupDto.Id}", groupDto);
    }

    private static async Task<IResult> UpdateGroup(
        Guid id,
        GroupDto groupDto,
        ApplicationDbContext db,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var isModerator = await db.GroupMembers
            .AnyAsync(gm => gm.GroupId == id && gm.UserId == userId && gm.IsModerator);

        if (!isModerator)
        {
            return TypedResults.Forbid();
        }

        var group = await db.Groups.FindAsync(id);

        if (group is null)
        {
            return TypedResults.NotFound();
        }

        group.Name = groupDto.Name;
        group.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    private static async Task<IResult> DeleteGroup(Guid id, ApplicationDbContext db, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var isModerator = await db.GroupMembers
            .AnyAsync(gm => gm.GroupId == id && gm.UserId == userId && gm.IsModerator);

        if (!isModerator)
        {
            return TypedResults.Forbid();
        }

        var group = await db.Groups.FindAsync(id);

        if (group is null)
        {
            return TypedResults.NotFound();
        }

        db.Groups.Remove(group);
        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    private static async Task<IResult> GetAllGroupMembers(Guid groupId, ApplicationDbContext db, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var isMember = await db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

        if (!isMember)
        {
            return TypedResults.Forbid();
        }

        var users = await db.GroupMembers
            .Where(gm => gm.GroupId == groupId)
            .Select(gm => new UserDto
            {
                Email = gm.User.Email!,
                DisplayName = gm.User.DisplayName,
                FirstName = gm.User.FirstName,
                LastName = gm.User.LastName
            })
            .ToListAsync();

        return TypedResults.Ok(users);
    }

    private static async Task<IResult> GetGroupMemberByEmail(
        Guid groupId,
        string email,
        ApplicationDbContext db,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var isMember = await db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

        if (!isMember)
        {
            return TypedResults.Forbid();
        }

        var userDto = await db.GroupMembers
            .Where(gm => gm.User.Email == email && gm.GroupId == groupId)
            .Select(gm => new UserDto
            {
                Email = gm.User.Email!,
                DisplayName = gm.User.DisplayName,
                FirstName = gm.User.FirstName,
                LastName = gm.User.LastName
            })
            .SingleOrDefaultAsync();

        if (userDto is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(userDto);
    }

    private static async Task<IResult> CreateGroupMember(
        Guid groupId,
        string email,
        ApplicationDbContext db,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var isModerator = await db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsModerator);

        if (!isModerator)
        {
            return TypedResults.Forbid();
        }

        var memberToAdd = await db.Users
            .SingleOrDefaultAsync(u => u.Email == email);

        if (memberToAdd is null)
        {
            return TypedResults.NotFound();
        }

        var alreadyMember = await db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == memberToAdd.Id);

        if (alreadyMember)
        {
            return TypedResults.BadRequest();
        }

        var groupMember = new GroupMember
        {
            GroupId = groupId,
            UserId = memberToAdd.Id,
            IsModerator = false,
            JoinedAt = DateTime.UtcNow
        };

        await db.GroupMembers.AddAsync(groupMember);
        await db.SaveChangesAsync();

        return TypedResults.Created($"/groups/{groupId}/members/{memberToAdd.Id}", new UserDto
        {
            Email = memberToAdd.Email!,
            DisplayName = memberToAdd.DisplayName,
            FirstName = memberToAdd.FirstName,
            LastName = memberToAdd.LastName
        });
    }

    private static async Task<IResult> DeleteGroupMember(
        Guid groupId,
        string email,
        ApplicationDbContext db,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var isModerator = await db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsModerator);

        if (!isModerator)
        {
            return TypedResults.Forbid();
        }

        var memberShipToDelete = await db.GroupMembers
            .Include(gm => gm.User)
            .SingleOrDefaultAsync(gm => gm.GroupId == groupId && gm.User.Email == email);

        if (memberShipToDelete is null)
        {
            return TypedResults.NotFound();
        }

        if (memberShipToDelete.IsModerator)
        {
            return TypedResults.BadRequest();
        }

        db.GroupMembers.Remove(memberShipToDelete);
        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    private static async Task<IResult> LeaveGroup(Guid groupId, ApplicationDbContext db, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var memberShipToDelete = await db.GroupMembers
            .SingleOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

        if (memberShipToDelete is null)
        {
            return TypedResults.NotFound();
        }

        if (memberShipToDelete.IsModerator)
        {
            return TypedResults.BadRequest();
        }

        db.GroupMembers.Remove(memberShipToDelete);
        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    private static async Task<IResult> GetAllGroupIssues(Guid groupId, ApplicationDbContext db, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var isMember = await db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

        if (!isMember)
        {
            return TypedResults.Forbid();
        }

        var issues = await db.Issues
            .Where(i => i.GroupId == groupId)
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

        return TypedResults.Ok(issues);
    }

    private static async Task<IResult> GetGroupIssueById(
        Guid groupId,
        Guid issueId,
        ApplicationDbContext db,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var isMember = await db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

        if (!isMember)
        {
            return TypedResults.Forbid();
        }

        var issue = await db.Issues.FindAsync(issueId);

        if (issue is null)
        {
            return TypedResults.NotFound();
        }

        if (issue.GroupId != groupId)
        {
            return TypedResults.Forbid();
        }

        return TypedResults.Ok(new IssueDto
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
    }

    private static async Task<IResult> CreateGroupIssue(
        Guid groupId,
        IssueDto issueDto,
        ApplicationDbContext db,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var isModerator = await db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsModerator);

        if (!isModerator)
        {
            return TypedResults.Forbid();
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
            CreatorId = userId,
            GroupId = groupId
        };

        var currentTime = DateTime.UtcNow;
        issue.CreatedAt = currentTime;
        issue.UpdatedAt = currentTime;

        await db.Issues.AddAsync(issue);
        await db.SaveChangesAsync();

        issueDto.Id = issue.Id;

        return TypedResults.Created($"/issue/{issueDto.Id}", issueDto);
    }

    private static async Task<IResult> UpdateGroupIssue(
        Guid groupId,
        Guid issueId,
        IssueDto issueDto,
        ApplicationDbContext db,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var isModerator = await db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsModerator);

        if (!isModerator)
        {
            return TypedResults.Forbid();
        }

        var issue = await db.Issues.FindAsync(issueId);

        if (issue is null)
        {
            return TypedResults.NotFound();
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

        return TypedResults.NoContent();
    }

    private static async Task<IResult> DeleteGroupIssue(
        Guid groupId,
        Guid issueId,
        ApplicationDbContext db,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var isModerator = await db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsModerator);

        if (!isModerator)
        {
            return TypedResults.Forbid();
        }

        var issue = await db.Issues.FindAsync(issueId);

        if (issue is null)
        {
            return TypedResults.NotFound();
        }

        db.Issues.Remove(issue);
        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    private static async Task<IResult> GetIssueAssignees(
        Guid groupId,
        Guid issueId,
        ApplicationDbContext db,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var isMember = await db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

        if (!isMember)
        {
            return TypedResults.Forbid();
        }

        var users = await db.Assignments
            .Where(a => a.IssueId == issueId)
            .Select(a => new UserDto
            {
                Email = a.User.Email!,
                DisplayName = a.User.DisplayName,
                FirstName = a.User.FirstName,
                LastName = a.User.LastName
            })
            .ToListAsync();

        return TypedResults.Ok(users);
    }

    private static async Task<IResult> CreateAssignment(
        Guid groupId,
        Guid issueId,
        string email,
        ApplicationDbContext db,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var isModerator = await db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsModerator);

        if (!isModerator)
        {
            return TypedResults.Forbid();
        }

        var memberToAdd = await db.Users
            .SingleOrDefaultAsync(u => u.Email == email);

        if (memberToAdd is null)
        {
            return TypedResults.NotFound();
        }

        var isAssignmentExists = await db.Assignments
            .AnyAsync(a => a.IssueId == issueId && a.UserId == memberToAdd.Id);

        if (isAssignmentExists)
        {
            return TypedResults.BadRequest();
        }

        var assignment = new Assignment
        {
            IssueId = issueId,
            UserId = memberToAdd.Id,
            AssignedAt = DateTime.UtcNow
        };

        await db.Assignments.AddAsync(assignment);
        await db.SaveChangesAsync();

        return TypedResults.Created($"/groups/{groupId}/issues/{issueId}/assignment", new UserDto
        {
            Email = memberToAdd.Email!,
            DisplayName = memberToAdd.DisplayName,
            FirstName = memberToAdd.FirstName,
            LastName = memberToAdd.LastName
        });
    }

    private static async Task<IResult> DeleteAssignment(
        Guid groupId,
        Guid issueId,
        string email,
        ApplicationDbContext db,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var isModerator = await db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsModerator);

        if (!isModerator)
        {
            return TypedResults.Forbid();
        }

        var memberId = await db.Users
            .Where(u => u.Email == email)
            .Select(u => u.Id)
            .SingleOrDefaultAsync();

        if (memberId is null)
        {
            return TypedResults.NotFound();
        }

        var assignment = await db.Assignments.FindAsync(issueId, memberId);

        if (assignment is null)
        {
            return TypedResults.NotFound();
        }

        db.Assignments.Remove(assignment);
        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}