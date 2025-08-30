using Microsoft.AspNetCore.Identity;
using ProjectManagement.Models;

public static class ApplicationDbInitializer
{
    public static async Task SeedRolesAndUsersAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        string[] roleNames = { "SuperAdmin", "Admin", "Manager", "Employee" };

        // Seed Roles
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Seed Users
        await CreateUserWithRole(userManager, "superadmin@example.com", "SuperAdmin123!", "SuperAdmin");
        await CreateUserWithRole(userManager, "admin@example.com", "Admin123!", "Admin");
        await CreateUserWithRole(userManager, "manager@example.com", "Manager123!", "Manager");
        await CreateUserWithRole(userManager, "employee@example.com", "Employee123!", "Employee");
    }

    private static async Task CreateUserWithRole(UserManager<ApplicationUser> userManager, string email, string password, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            var newUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Name = role + " User",
                PhoneNo = "01234567890"
            };

            var result = await userManager.CreateAsync(newUser, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newUser, role);
            }
            else
            {
                // Handle creation errors
                throw new Exception($"Failed to create {role} user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}
