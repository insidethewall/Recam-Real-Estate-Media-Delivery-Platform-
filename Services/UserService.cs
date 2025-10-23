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
    private readonly ILogger<UserService> _logger;

    public UserService(ReacmDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IJwtTokenService jwtTokenService, IUserRepository userRepository, IEmailSender emailSender, ILogger<UserService> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenService = jwtTokenService;
        _userRepository = userRepository;
        _emailSender = emailSender;
        _logger = logger;
    }


    public async Task<string> CreateAgentAsync(AgentCreateDto registerRequest, string currentUserId)
    {
        string password = PasswordGenerator.GenerateStrongPassword();
        _logger.LogInformation($"Generated password for new agent: {password}");

        User user = new User
        {
            Email = registerRequest.Email,
            UserName = registerRequest.Email,
            CreatedAt = DateTime.UtcNow
        };
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

    public async Task<AgentPhotographer> AddAgentAsync(User currentUser, User agentUser)
    {
        AgentPhotographer agentPhotographer = await _userRepository.CreateAgentPhotographerAsync(currentUser, agentUser);
        return agentPhotographer;
    }


    // only Admin can delete users
    public async Task<ApiResponse<object?>> DeleteUserAsync(string currentUserId, string targetUserId)
    {
        User? currentUser = await _userManager.FindByIdAsync(currentUserId);
        User? targetUser = await _userManager.FindByIdAsync(targetUserId);
        if (currentUser == null)
            return ApiResponse<object?>.Fail("Current user Unauthorize.", "401");
        if (targetUser == null)
            return ApiResponse<object?>.Fail("User not found.", "404");

        var targetRoles = await _userManager.GetRolesAsync(targetUser);
        var currentRoles = await _userManager.GetRolesAsync(currentUser);
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
            ICollection<AgentPhotographer> deletedAgentPhotographers = await _userRepository.DeleteAgentPhotographerCompany(targetUser);

// Delete Photographer, MediaAssets
            if (targetRole == Role.Photographer.ToString())
            {
            }
            else if (targetRole == Role.Agent.ToString())
            {
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



    public async Task<ICollection<UserInfoDto>> GetAllAgentsAsync()
    {
        return await _userRepository.GetAllAgentsAsync();
    }

    public async Task<ICollection<UserInfoDto>> GetAllPhotographersAsync()
    {
        return await _userRepository.GetAllPhotographersAsync();
    }

    public async Task<ICollection<UserInfoDto>> GetAgentsByPhotographerAsync(string photographerId)
    {
        if (string.IsNullOrEmpty(photographerId))
            throw new ArgumentException("Photographer ID cannot be null or empty.", nameof(photographerId));
        return await _userRepository.GetAgentsByPhotographerAsync(photographerId);
    }
    

}
