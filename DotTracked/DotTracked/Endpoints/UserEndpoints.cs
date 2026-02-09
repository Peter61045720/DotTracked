using System.Security.Claims;
using DotTracked.Data;
using DotTracked.Shared.DTOs;
using Microsoft.AspNetCore.Identity;

namespace DotTracked.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var userGroup = app.MapGroup("/api/user").RequireAuthorization();

        userGroup.MapGet("/details",
            async (
                UserManager<ApplicationUser> userManager,
                ClaimsPrincipal user) =>
            {
                var appUser = await userManager.GetUserAsync(user);

                if (appUser is null)
                {
                    return Results.Unauthorized();
                }

                return Results.Ok(new UserDto
                {
                    Email = appUser.Email!,
                    DisplayName = appUser.DisplayName,
                    FirstName = appUser.FirstName,
                    LastName = appUser.LastName
                });
            });

        userGroup.MapPut("/",
            async (
                UserManager<ApplicationUser> userManager,
                ClaimsPrincipal user,
                UserDto userDto) =>
            {
                var appUser = await userManager.GetUserAsync(user);

                if (appUser is null)
                {
                    return Results.Unauthorized();
                }

                appUser.DisplayName = userDto.DisplayName;
                appUser.FirstName = userDto.FirstName;
                appUser.LastName = userDto.LastName;
                appUser.UpdatedAt = DateTime.UtcNow;

                var result = await userManager.UpdateAsync(appUser);

                return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
            });

        userGroup.MapPost("/delete-account", async (
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ClaimsPrincipal user,
            PasswordDto passwordDto) =>
        {
            var appUser = await userManager.GetUserAsync(user);

            if (appUser is null)
            {
                return Results.Unauthorized();
            }

            var isPasswordValid = await userManager.CheckPasswordAsync(appUser, passwordDto.Password);

            if (!isPasswordValid)
            {
                return Results.BadRequest("The password you entered is incorrect");
            }

            var result = await userManager.DeleteAsync(appUser);

            if (!result.Succeeded)
            {
                Results.BadRequest(result.Errors);
            }

            await signInManager.SignOutAsync();

            return Results.Ok();
        });
    }
}