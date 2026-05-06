using System.Net;
using System.Net.Http.Json;
using DotTracked.Data;
using DotTracked.Data.Models;
using DotTracked.IntegrationTests.Infrastructure;
using DotTracked.Shared.DTOs;

namespace DotTracked.IntegrationTests.Endpoints;

public class AdminEndpointsTests(WebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Get_AllUsers_ReturnsAllUsersExceptAdmins_WhenUserIsAdmin()
    {
        // Arrange
        await AddAsync(new ApplicationUser { UserName = "john.doe@email.com", Email = "john.doe@email.com" });
        await AddAsync(new ApplicationUser { UserName = "jane.doe@email.com", Email = "jane.doe@email.com" });

        var http = CreateAdminClient();

        // Act
        var response = await http.GetAsync("/api/admin/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var users = await response.Content.ReadFromJsonAsync<List<AdminUserDto>>();

        Assert.NotNull(users);
        Assert.DoesNotContain(users, u => u.Id == CurrentUserId);
        Assert.True(users.Count >= 2);
    }

    [Fact]
    public async Task Get_AllUsers_ReturnsForbidden_WhenUserIsAppUser()
    {
        // Arrange
        var http = CreateAppUserClient();

        // Act
        var response = await http.GetAsync("/api/admin/users");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_UserById_ReturnsUser_WhenUserIsAdmin()
    {
        // Arrange
        var user = await AddAsync(new ApplicationUser
        {
            UserName = "john.doe1@email.com", Email = "john.doe1@email.com"
        });

        var http = CreateAdminClient();

        // Act
        var response = await http.GetAsync($"/api/admin/users/{user.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var foundUser = await response.Content.ReadFromJsonAsync<AdminUserDto>();

        Assert.Equal(user.Id, foundUser!.Id);
    }

    [Fact]
    public async Task Get_UserById_ReturnsForbidden_WhenUserIsAppUser()
    {
        // Arrange
        var http = CreateAppUserClient();

        // Act
        var response = await http.GetAsync($"/api/admin/users/{Guid.NewGuid().ToString()}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_UserById_ReturnsNotFound_WhenUserDoesNotExists()
    {
        // Arrange
        var http = CreateAdminClient();

        // Act
        var response = await http.GetAsync($"/api/admin/users/{Guid.NewGuid().ToString()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_User_ReturnsNoContent_WhenUserIsAdmin()
    {
        // Arrange
        var userToDelete = await AddAsync(new ApplicationUser
        {
            UserName = "john.doe2@email.com", Email = "john.doe2@email.com"
        });

        var http = CreateAdminClient();

        // Act
        var response = await http.DeleteAsync($"/api/admin/users/{userToDelete.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_User_ReturnsForbidden_WhenUserIsAppUser()
    {
        // Arrange
        var http = CreateAppUserClient();

        // Act
        var response = await http.DeleteAsync($"/api/admin/users/{Guid.NewGuid().ToString()}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_User_ReturnsNotFound_WhenUserDoesNotExists()
    {
        // Arrange
        var http = CreateAdminClient();

        // Act
        var response = await http.DeleteAsync($"/api/admin/users/{Guid.NewGuid().ToString()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_AllGroups_ReturnsAllGroups_WhenUserIsAdmin()
    {
        // Arrange
        await AddAsync(new Group { Name = "Test Group 1" });
        await AddAsync(new Group { Name = "Test Group 2" });

        var http = CreateAdminClient();

        // Act
        var response = await http.GetAsync("/api/admin/groups");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var groups = await response.Content.ReadFromJsonAsync<List<AdminGroupDto>>();

        Assert.NotNull(groups);
        Assert.True(groups.Count >= 2);
    }

    [Fact]
    public async Task Get_AllGroups_ReturnsForbidden_WhenUserIsAppUser()
    {
        // Arrange
        var http = CreateAppUserClient();

        // Act
        var response = await http.GetAsync("/api/admin/groups");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_GroupById_ReturnsGroup_WhenUserIsAdmin()
    {
        // Arrange
        var group = await AddAsync(new Group { Name = "Test Group 3" });

        var http = CreateAdminClient();

        // Act
        var response = await http.GetAsync($"/api/admin/groups/{group.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var foundGroup = await response.Content.ReadFromJsonAsync<AdminGroupDto>();

        Assert.Equal(group.Id, foundGroup!.Id);
    }

    [Fact]
    public async Task Get_GroupById_ReturnsForbidden_WhenUserIsAppUser()
    {
        // Arrange
        var http = CreateAppUserClient();

        // Act
        var response = await http.GetAsync($"/api/admin/groups/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_GroupById_ReturnsNotFound_WhenGroupDoesNotExists()
    {
        // Arrange
        var http = CreateAdminClient();

        // Act
        var response = await http.GetAsync($"/api/admin/groups/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Group_ReturnsNoContent_WhenUserIsAdmin()
    {
        // Arrange
        var groupToDelete = await AddAsync(new Group { Name = "Test Group 4" });

        var http = CreateAdminClient();

        // Act
        var response = await http.DeleteAsync($"/api/admin/groups/{groupToDelete.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Group_ReturnsForbidden_WhenUserIsAppUser()
    {
        // Arrange
        var http = CreateAppUserClient();

        // Act
        var response = await http.DeleteAsync($"/api/admin/groups/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Group_ReturnsNotFound_WhenGroupDoesNotExists()
    {
        // Arrange
        var http = CreateAdminClient();

        // Act
        var response = await http.DeleteAsync($"/api/admin/groups/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_AllGroupMembers_ReturnsAllGroupMembers_WhenUserIsAdmin()
    {
        // Arrange
        var group = await AddAsync(new Group { Name = "Test Group With Members 1" });

        var moderator = await AddAsync(new ApplicationUser
        {
            UserName = "group.moderator@email.com", Email = "group.moderator@email.com"
        });

        var member = await AddAsync(new ApplicationUser
        {
            UserName = "group.member@email.com", Email = "group.member@email.com"
        });

        await AddAsync(new GroupMember { GroupId = group.Id, UserId = moderator.Id, IsModerator = true });
        await AddAsync(new GroupMember { GroupId = group.Id, UserId = member.Id });

        var http = CreateAdminClient();

        // Act
        var response = await http.GetAsync($"/api/admin/groups/{group.Id}/members");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var members = await response.Content.ReadFromJsonAsync<List<AdminGroupMemberDto>>();

        Assert.Equal(2, members!.Count);
        Assert.Contains(members, gm => gm.GroupId == group.Id && gm.UserId == moderator.Id && gm.IsModerator);
        Assert.Contains(members, gm => gm.GroupId == group.Id && gm.UserId == member.Id && !gm.IsModerator);
    }

    [Fact]
    public async Task Get_AllGroupMembers_ReturnsForbidden_WhenUserIsAppUser()
    {
        // Arrange
        var http = CreateAppUserClient();

        // Act
        var response = await http.GetAsync($"/api/admin/groups/{Guid.NewGuid()}/members");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_GroupMemberById_ReturnsGroupMember_WhenUserIsAdmin()
    {
        // Arrange
        var group = await AddAsync(new Group { Name = "Test Group With Members 2" });

        var moderator = await AddAsync(new ApplicationUser
        {
            UserName = "group2.moderator@email.com", Email = "group2.moderator@email.com"
        });

        await AddAsync(new GroupMember { GroupId = group.Id, UserId = moderator.Id, IsModerator = true });

        var http = CreateAdminClient();

        // Act
        var response = await http.GetAsync($"/api/admin/groups/{group.Id}/members/{moderator.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var foundGroupMember = await response.Content.ReadFromJsonAsync<AdminGroupMemberDto>();

        Assert.True(foundGroupMember!.GroupId == group.Id
                    && foundGroupMember.UserId == moderator.Id
                    && foundGroupMember.IsModerator);
    }

    [Fact]
    public async Task Get_GroupMemberById_ReturnsForbidden_WhenUserIsAppUser()
    {
        // Arrange
        var http = CreateAppUserClient();

        // Act
        var response = await http.GetAsync($"/api/admin/groups/{Guid.NewGuid()}/members/{Guid.NewGuid().ToString()}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_GroupMemberById_ReturnsNotFound_WhenGroupMembershipDoesNotExists()
    {
        // Arrange
        var http = CreateAdminClient();

        // Act
        var response = await http.GetAsync($"/api/admin/groups/{Guid.NewGuid()}/members/{Guid.NewGuid().ToString()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_GroupMembership_ReturnsCreated_WhenUserIsAdmin()
    {
        // Arrange
        var group = await AddAsync(new Group { Name = "Test Group With Members 3" });

        var member = await AddAsync(new ApplicationUser
        {
            UserName = "group3.member@email.com", Email = "group3.member@email.com"
        });

        var http = CreateAdminClient();

        // Act
        var response = await http.PostAsync($"/api/admin/groups/{group.Id}/members/{member.Id}", null);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdGroupMember = await response.Content.ReadFromJsonAsync<AdminGroupMemberDto>();

        Assert.True(createdGroupMember!.GroupId == group.Id && createdGroupMember.UserId == member.Id);
    }

    [Fact]
    public async Task Post_GroupMembership_ReturnsForbidden_WhenUserIsAppUser()
    {
        // Arrange
        var http = CreateAppUserClient();

        // Act
        var response =
            await http.PostAsync($"/api/admin/groups/{Guid.NewGuid()}/members/{Guid.NewGuid().ToString()}", null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Post_GroupMembership_ReturnsNotFound_WhenMembershipDoesNotExists()
    {
        // Arrange
        var http = CreateAdminClient();

        // Act
        var response =
            await http.PostAsync($"/api/admin/groups/{Guid.NewGuid()}/members/{Guid.NewGuid().ToString()}", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_GroupMembership_ReturnsBadRequest_WhenUserIsAlreadyMember()
    {
        // Arrange
        var group = await AddAsync(new Group { Name = "Test Group With Members 4" });

        var moderator = await AddAsync(new ApplicationUser
        {
            UserName = "group4.moderator@email.com", Email = "group4.moderator@email.com"
        });

        await AddAsync(new GroupMember { GroupId = group.Id, UserId = moderator.Id, IsModerator = true });

        var http = CreateAdminClient();

        // Act
        var response = await http.PostAsync($"/api/admin/groups/{group.Id}/members/{moderator.Id}", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_GroupModeratorStatus_ReturnsNoContent_WhenUserIsAdmin()
    {
        // Arrange
        var group = await AddAsync(new Group { Name = "Test Group With Members 5" });

        var moderator = await AddAsync(new ApplicationUser
        {
            UserName = "group5.moderator@email.com", Email = "group5.moderator@email.com"
        });

        var member = await AddAsync(new ApplicationUser
        {
            UserName = "group5.member@email.com", Email = "group5.member@email.com"
        });

        await AddAsync(new GroupMember { GroupId = group.Id, UserId = moderator.Id, IsModerator = true });
        await AddAsync(new GroupMember { GroupId = group.Id, UserId = member.Id });

        var http = CreateAdminClient();

        // Act
        var response =
            await http.PutAsync($"/api/admin/groups/{group.Id}/members/{member.Id}/moderator?isModerator={true}", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var updatedMember = await FindAsync<GroupMember>(group.Id, member.Id);

        Assert.NotNull(updatedMember);
        Assert.True(updatedMember.IsModerator);
    }

    [Fact]
    public async Task Put_GroupModeratorStatus_ReturnsForbidden_WhenUserIsAppUser()
    {
        // Arrange
        var http = CreateAppUserClient();

        // Act
        var response = await http.PutAsync(
            $"/api/admin/groups/{Guid.NewGuid()}/members/{Guid.NewGuid().ToString()}/moderator?isModerator={true}",
            null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Put_GroupModeratorStatus_ReturnsNotFound_WhenMembershipDoesNotExists()
    {
        // Arrange
        var http = CreateAdminClient();

        // Act
        var response = await http.PutAsync(
            $"/api/admin/groups/{Guid.NewGuid()}/members/{Guid.NewGuid().ToString()}/moderator?isModerator={true}",
            null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_GroupMember_ReturnsNoContent_WhenUserIsAdmin()
    {
        // Arrange
        var group = await AddAsync(new Group { Name = "Test Group With Members 6" });

        var member = await AddAsync(new ApplicationUser
        {
            UserName = "group6.member@email.com", Email = "group6.member@email.com"
        });

        await AddAsync(new GroupMember { GroupId = group.Id, UserId = member.Id });

        var http = CreateAdminClient();

        // Act
        var response = await http.DeleteAsync($"/api/admin/groups/{group.Id}/members/{member.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var deletedMembership = await FindAsync<GroupMember>(group.Id, member.Id);

        Assert.Null(deletedMembership);
    }

    [Fact]
    public async Task Delete_GroupMember_ReturnsForbidden_WhenUserIsAppUser()
    {
        // Arrange
        var http = CreateAppUserClient();

        // Act
        var response =
            await http.DeleteAsync($"/api/admin/groups/{Guid.NewGuid()}/members/{Guid.NewGuid().ToString()}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_GroupMember_ReturnsNotFound_WhenMembershipDoesNotExists()
    {
        // Arrange
        var http = CreateAdminClient();

        // Act
        var response =
            await http.DeleteAsync($"/api/admin/groups/{Guid.NewGuid()}/members/{Guid.NewGuid().ToString()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_GroupMember_ReturnsBadRequest_WhenTheMemberIsModerator()
    {
        // Arrange
        var group = await AddAsync(new Group { Name = "Test Group With Members 7" });

        var moderator = await AddAsync(new ApplicationUser
        {
            UserName = "group7.moderator@email.com", Email = "group7.moderator@email.com"
        });

        await AddAsync(new GroupMember { GroupId = group.Id, UserId = moderator.Id, IsModerator = true });

        var http = CreateAdminClient();

        // Act
        var response = await http.DeleteAsync($"/api/admin/groups/{group.Id}/members/{moderator.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var membership = await FindAsync<GroupMember>(group.Id, moderator.Id);

        Assert.NotNull(membership);
        Assert.True(membership.IsModerator);
    }
}