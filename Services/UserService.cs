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
    private readonly IGeneralRepository _generalRepository;
    private readonly IAzureBlobStorageService _blobStorageService;
    private readonly IUserLogRepository _userLogRepository;

    public UserService(ReacmDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IJwtTokenService jwtTokenService, IUserRepository userRepository, IEmailSender emailSender, ILogger<UserService> logger, IGeneralRepository generalRepository, IAzureBlobStorageService blobStorageService, IUserLogRepository userLogRepository)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenService = jwtTokenService;
        _userRepository = userRepository;
        _emailSender = emailSender;
        _logger = logger;
        _generalRepository = generalRepository;
        _blobStorageService = blobStorageService;
        _userLogRepository = userLogRepository;
    }


    public async Task<string> CreateAgentAsync(AgentCreateDto registerRequest, string currentUserId)
    {
        string password = PasswordGenerator.GenerateStrongPassword();
        _logger.LogInformation($"Generated password for new agent: {password}");
        User? currentUser = await _userManager.FindByIdAsync(currentUserId);
          if (currentUser == null)
            throw new System.Exception("Current user not found.");
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
            string? avatarUrl = null;
            if (registerRequest.Avatar != null)
            { 
                 avatarUrl =  await _blobStorageService.UploadFileAsync(registerRequest.Avatar, "Agent-avatars");
            }

            Agent agent = _generalRepository.MapDto<IUserProfileDto, Agent>(registerRequest);
            agent.Id = user.Id;
            agent.User = user;
            agent.AvatarUrl = avatarUrl; 

            await _userRepository.CreateAgentAsync(agent, user);
            await _generalRepository.SaveChangesAsync();
            await transaction.CommitAsync();
            string token = await _jwtTokenService.GenerateTokenAsync(user);
            await _emailSender.SendEmailAsync(
                registerRequest.Email,
                "Welcome to Recam System",
                $"Your account has been created successfully. Your password is: {password}. Please change it after your first login."
            );

            UserLog userLog = _userLogRepository.CreateUserLog(currentUser, UserAction.CreateAgent, user, "create agent");
            await _userLogRepository.AddLog(userLog);
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
        await _generalRepository.SaveChangesAsync();
        return agentPhotographer;
    }


    // only Admin can delete users
    public async Task<UserDeletionDto> DeleteUserAsync(string currentUserId, string targetUserId)
    {
        User? currentUser = await _userManager.FindByIdAsync(currentUserId);
        User? targetUser = await _userManager.FindByIdAsync(targetUserId);
        if (currentUser == null)
            throw new System.Exception("Current user not found.");
        if (targetUser == null)
           throw new System.Exception("Target user not found.");

        var targetRoles = await _userManager.GetRolesAsync(targetUser);
        var currentRoles = await _userManager.GetRolesAsync(currentUser);
        var targetRole = targetRoles.FirstOrDefault();
        var currentRole = currentRoles.FirstOrDefault();
        if (currentRole == null || targetRole == null)
            throw new System.Exception("User roles not found.");
        // Check permissions
        if (currentRole == Role.Agent.ToString() || currentRole == Role.Photographer.ToString())
            throw new System.Exception("You do not have permission to delete users.");
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            targetUser.IsDeleted = true; // Soft delete
            IdentityResult result = await _userManager.UpdateAsync(targetUser);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new System.Exception($"Failed to delete user: {errors}");
            }
            // Already cascade delete related data in the database
            // ICollection<AgentPhotographer> deletedAgentPhotographers = await _userRepository.DeleteAgentPhotographerCompany(targetUser);
            UserDeletionDto userDeletionDto = new UserDeletionDto
            {
                user = targetUser,
            };

            // Delete Photographer, MediaAssets
            if (targetRole == Role.Photographer.ToString())
            {

                Photographer photographer = _userRepository.DeletePhotographer(targetUser);
                if (!string.IsNullOrEmpty(photographer.AvatarUrl) )
                    await _blobStorageService.DeleteFileAsync(photographer.AvatarUrl);
                userDeletionDto.photographer = photographer;
            }
            else if (targetRole == Role.Agent.ToString())
            {
                Agent agent = _userRepository.DeleteAgent(targetUser);
                if (!string.IsNullOrEmpty(agent.AvatarUrl))
                    await _blobStorageService.DeleteFileAsync(agent.AvatarUrl);
                userDeletionDto.agent = agent;
            }
            await _generalRepository.SaveChangesAsync();
            await transaction.CommitAsync();
            UserLog userLog = _userLogRepository.CreateUserLog(currentUser, UserAction.DeleteUser, targetUser, "Delete user");
            await _userLogRepository.AddLog(userLog);
            return userDeletionDto;

        }
        catch (System.Exception ex)
        {
            await transaction.RollbackAsync();
            targetUser.IsDeleted = false;
            IdentityResult result = await _userManager.UpdateAsync(targetUser);
            throw new System.Exception($"Error deleting user: {ex.Message}");
            
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
