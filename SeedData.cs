using Furni.Models;
using Microsoft.AspNetCore.Identity;

public class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Tạo Role ADMIN nếu chưa tồn tại
        if (!await roleManager.RoleExistsAsync("ADMIN"))
        {
            await roleManager.CreateAsync(new IdentityRole("ADMIN"));
        }

        // Tạo Role USER nếu chưa tồn tại
        if (!await roleManager.RoleExistsAsync("USER"))
        {
            await roleManager.CreateAsync(new IdentityRole("USER"));
        }

        // Tạo tài khoản ADMIN nếu cần
        var adminUser = await userManager.FindByEmailAsync("admin@gmail.com");
        if (adminUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                Address = "thai binh",
                FullName = "Admin",
                PhoneNumber = "0123456789",
            };

            var result = await userManager.CreateAsync(user, "Abc123!@#");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "ADMIN");
            }
        }
    }
}