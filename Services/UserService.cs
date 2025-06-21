using Microsoft.AspNetCore.Identity;

using RecamSystemApi.Data;
using RecamSystemApi.DTOs;
using RecamSystemApi.Enums;
using RecamSystemApi.Helper;
using RecamSystemApi.Models;
using RecamSystemApi.Utility;

namespace RecamSystemApi.Services;

public class UserService : IUserService
{
    private readonly ReacmDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;

    public UserService(ReacmDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IJwtTokenService jwtTokenService, IUserRepository userRepository, IEmailSender emailSender)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenService = jwtTokenService;
        _userRepository = userRepository;
        _emailSender = emailSender;
    }


    public async Task<string> CreateAgentAsync(AgentCreateDto registerRequest, string currentUserId)
    {
        string password = PasswordGenerator.GenerateStrongPassword();
        User user = new User
        {
            Email = registerRequest.Email,
            UserName = registerRequest.Email,
            CreatedAt = DateTime.UtcNow
        };
        Console.WriteLine($"UserName: {user.UserName}");
        var roleName = Role.Agent.ToString();
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
                throw new System.Exception("Role does not exist.");

            IdentityResult result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new System.Exception($"User creation failed: {errors}");
            }

            await _userManager.AddToRoleAsync(user, roleName);
            await _userRepository.CreateAgentAsync(registerRequest, user);
            await transaction.CommitAsync();
            string token = await _jwtTokenService.GenerateTokenAsync(user);
            await _emailSender.SendEmailAsync(
                registerRequest.Email,
                "Welcome to Recam System",
                $"Your account has been created successfully. Your password is: {password}. Please change it after your first login."
            );
            return token;
        }
        catch (System.Exception ex)
        {
            await transaction.RollbackAsync();
            await _userManager.DeleteAsync(user); // Clean up in case of error
            throw new System.Exception($"Internal server error: {ex.Message}");
        }
    }


    // only Admin can delete users
    public async Task<ApiResponse<object?>> DeleteUserAsync(string currentUserId, string targetUserId)
    {
        User? currentUser = await _userManager.FindByIdAsync(currentUserId);
        Console.WriteLine($"Current User : {currentUser?.UserName}");
        User? targetUser = await _userManager.FindByIdAsync(targetUserId);
        if (currentUser == null)
            return ApiResponse<object?>.Fail("Current user Unauthorize.", "401");
        if (targetUser == null)
            return ApiResponse<object?>.Fail("User not found.", "404");

        var targetRoles = await _userManager.GetRolesAsync(targetUser);
        var currentRoles = await _userManager.GetRolesAsync(currentUser);
        Console.WriteLine($"Current User Roles: {string.Join(", ", currentRoles)}");
        var targetRole = targetRoles.FirstOrDefault();
        var currentRole = currentRoles.FirstOrDefault();
        if (currentRole == null || targetRole == null)
            return ApiResponse<object?>.Fail("User roles not found.", "404");
        // Check permissions
        if (currentRole == Role.Agent.ToString() || currentRole == Role.Photographer.ToString())
            return ApiResponse<object?>.Fail($"{currentRole} are not allowed to delete any users.", "403");
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            targetUser.IsDeleted = true; // Soft delete
            IdentityResult result = await _userManager.UpdateAsync(targetUser);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return ApiResponse<object?>.Fail($"User deletion failed: {errors}", "500");
            }
            if (targetRole == Role.Photographer.ToString())
            {
                await _userRepository.DeleteAgentPhotographerCompany(targetUserId);
            }
            else if (targetRole == Role.Agent.ToString())
            {
                await _userRepository.DeleteAgentPhotographerCompany(targetUserId);
                //ToDo: Delete AgentListing cases
            }
            await transaction.CommitAsync();
            return ApiResponse<object?>.Success(targetUser, "User deleted successfully.");

        }
        catch (System.Exception ex)
        {
            await transaction.RollbackAsync();
            targetUser.IsDeleted = false;
            IdentityResult result = await _userManager.UpdateAsync(targetUser);
            return ApiResponse<object?>.Fail($"Internal server error: {ex.Message}", "500");
        }
    }

    public async Task<ApiResponse<object?>> AddAgentAsync(string agentEmail, string currentUserId)
    {
        User? currentUser = await _userManager.FindByIdAsync(currentUserId);
        if (currentUser == null)
            return ApiResponse<object?>.Fail("Current user not found.", "404");

        if (!await _userManager.IsInRoleAsync(currentUser, Role.Photographer.ToString()))
            return ApiResponse<object?>.Fail("Only photographers can add agents.", "403");

        User? agentUser = await _userManager.FindByEmailAsync(agentEmail);
        if (agentUser == null)
            return ApiResponse<object?>.Fail("Agent not found.", "404");

        if (!await _userManager.IsInRoleAsync(agentUser, Role.Agent.ToString()))
            return ApiResponse<object?>.Fail("This user is not an agent.", "403");
        try
        {
            await _userRepository.CreateAgentPhotographerAsync(currentUser.Id, agentUser.Id);
            return ApiResponse<object?>.Success(agentUser, "Agent added successfully.");
        }
        catch (System.Exception ex)
        {
            return ApiResponse<object?>.Fail($"Error adding agent: {ex.Message}", "500");
        }
    }

    public async Task<ICollection<Agent>> GetAllAgentsAsync()
    {
        return await _userRepository.GetAllAgentsAsync();
    }

    public async Task<ICollection<Photographer>> GetAllPhotographersAsync()
    {
        return await _userRepository.GetAllPhotographersAsync();
    }

    public async Task<ICollection<AgentInfoDto>> GetAgentsByPhotographerAsync(string photographerId)
    {
        if (string.IsNullOrEmpty(photographerId))
            throw new ArgumentException("Photographer ID cannot be null or empty.", nameof(photographerId));
        return await _userRepository.GetAgentsByPhotographerAsync(photographerId);
    }
    

}
