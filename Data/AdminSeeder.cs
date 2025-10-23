using Microsoft.AspNetCore.Identity;
using RecamSystemApi.DTOs;
using RecamSystemApi.Enums;
using RecamSystemApi.Models;

public static class AdminSeeder
{
    public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        string password = "Admin@123";
        User register = new User
        {
            Email = "rxy199@qq.com",
            UserName = "rxy199@qq.com",
            CreatedAt = DateTime.UtcNow,
        };
        var user = await userManager.FindByEmailAsync(register.Email);
        if (user != null)
        {
            return;
        }
        var result = await userManager.CreateAsync(register, password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new UserRegistrationException($"User creation failed: {errors}");
        }
        string admin = Role.Admin.ToString();
        await userManager.AddToRoleAsync(register, admin);
        
        
    } 
    
}



