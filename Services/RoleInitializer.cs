using Furni.Models;
using Microsoft.AspNetCore.Identity;

namespace Furni.Services;

public class RoleInitializer
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleInitializer(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task InitializeAsync()
    {
        // Create roles if they don't exist
        string[] roleNames = { "ADMIN", "USER" };
        foreach (var roleName in roleNames)
        {
            var roleExist = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Check if admin user exists, if not create it
        var adminUser = await _userManager.FindByEmailAsync("admin@gmail.com");
        if (adminUser == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                FullName = "Administrator",
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(admin, "Admin@123"); // Strong default password
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(admin, "ADMIN");
            }
        }
        else
        {
            // Ensure existing admin user has ADMIN role
            if (!await _userManager.IsInRoleAsync(adminUser, "ADMIN"))
            {
                await _userManager.AddToRoleAsync(adminUser, "ADMIN");
            }
        }
    }
}
