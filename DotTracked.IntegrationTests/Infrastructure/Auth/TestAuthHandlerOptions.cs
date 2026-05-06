using DotTracked.Shared.Constants;
using Microsoft.AspNetCore.Authentication;

namespace DotTracked.IntegrationTests.Infrastructure.Auth;

public class TestAuthHandlerOptions : AuthenticationSchemeOptions
{
    public string DefaultUserId { get; set; } = Guid.NewGuid().ToString();
    public string DefaultRole { get; set; } = Roles.AppUser;
}