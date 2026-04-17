using DotTracked.Shared.Constants;
using Microsoft.AspNetCore.Identity;

namespace DotTracked.Data;

public static class DbInitializer
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider, IConfiguration config)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roleNames = [Roles.Admin, Roles.AppUser];

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        var adminEmail = config.GetValue<string>("AdminData:Email");
        var adminPassword = config.GetValue<string>("AdminData:Password");

        if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword) &&
            await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var currentTime = DateTime.UtcNow;

            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = "Admin",
                FirstName = "Administrator",
                LastName = "dotTRACKED",
                CreatedAt = currentTime,
                UpdatedAt = currentTime
            };

            var result = await userManager.CreateAsync(admin, adminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, Roles.Admin);
            }
        }
    }
}