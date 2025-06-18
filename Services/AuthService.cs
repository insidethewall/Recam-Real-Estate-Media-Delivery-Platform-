using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RecamSystemApi.Data;
using RecamSystemApi.DTOs;
using RecamSystemApi.Enums;
using RecamSystemApi.Helper;
using RecamSystemApi.Models;
using RecamSystemApi.Utility;
using IEmailSender = RecamSystemApi.Helper.IEmailSender;


namespace RecamSystemApi.Services;

public class AuthService : IAuthService
{
    private readonly ReacmDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuthRepository _authRepository;
    private readonly IEmailSender _emailSender;
    private readonly IMapper _mapper;


    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="userManager"></param>
    public AuthService(ReacmDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IJwtTokenService jwtTokenService, IAuthRepository authRepository, IEmailSender emailSender, IMapper mapper)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenService = jwtTokenService;
        _authRepository = authRepository;
        _emailSender = emailSender;
        _mapper = mapper;

    }

// only for Admin and Photographer roles
    public async Task<string> Register(RegisterRequestDto registerRequest)
    {
        if (registerRequest.Role == Role.Agent)
            throw new System.Exception("Agent accounts must be created by an Admin or Photographer.");
        User user = new User
        {
            Email = registerRequest.Email,
            UserName = registerRequest.Email,
            CreatedAt = DateTime.UtcNow,
        };
        Console.WriteLine($"UserName: {user.UserName}");
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        { 
            var roleName = registerRequest.Role.ToString();

            if (!await _roleManager.RoleExistsAsync(roleName))
                throw new System.Exception("Role does not exist.");

            IdentityResult result = await _userManager.CreateAsync(user, registerRequest.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new System.Exception($"User creation failed: {errors}");
            }

            await _userManager.AddToRoleAsync(user, roleName);
            if (registerRequest.Role == Role.Photographer)
            { 
                await _authRepository.AddPhotographerAsync(registerRequest, user);
            }
            await transaction.CommitAsync();
            string token = await _jwtTokenService.GenerateTokenAsync(user);
            return token;
        } catch (System.Exception ex)
        {
            await transaction.RollbackAsync();
            await _userManager.DeleteAsync(user); // Clean up in case of error
            throw new System.Exception($"Internal server error: {ex.Message}");
        }
   
    }

// login for all roles
    public async Task<string> Login(LoginRequestDto loginRequest)
    {
        var user = await _userManager.FindByEmailAsync(loginRequest.Email);
        if (user == null)
            throw new System.Exception("Invalid email or password.");

        bool isPasswordValid = await _userManager.CheckPasswordAsync(user, loginRequest.Password);
        if (!isPasswordValid)
            throw new System.Exception("Invalid email or password.");

        return await _jwtTokenService.GenerateTokenAsync(user);
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

        IdentityResult result = await _userManager.CreateAsync(user,password);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new System.Exception($"User creation failed: {errors}");
        }

        await _userManager.AddToRoleAsync(user, roleName);
        await _authRepository.AddAgentAsync(registerRequest, user);
        await _authRepository.AddAgentPhotographerAsync(currentUserId, user.Id);
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

    public async Task<ApiResponse<object?>> DeleteUserAsync(string currentUserId, string targetUserId)
    {
        User? currentUser = await _userManager.FindByIdAsync(currentUserId);
        Console.WriteLine( $"Current User : {currentUser?.UserName}");
        User? targetUser = await _userManager.FindByIdAsync(targetUserId);
        if (currentUser == null)
            return ApiResponse<object?>.Fail("Current user Unauthorize.", "401");
        if (targetUser == null)
            return ApiResponse<object?>.Fail("User not found.", "404");

        var targetRoles = await _userManager.GetRolesAsync(targetUser);
        var currentRoles = await _userManager.GetRolesAsync(currentUser);
        Console.WriteLine( $"Current User Roles: {string.Join(", ", currentRoles)}");
        var targetRole = targetRoles.FirstOrDefault();
        var currentRole = currentRoles.FirstOrDefault();
        if (currentRole == null || targetRole == null)
            return ApiResponse<object?>.Fail("User roles not found.", "404");
        // Check permissions
        if (currentRole == Role.Agent.ToString())
            return ApiResponse<object?>.Fail("Agents are not allowed to delete any users.", "403");

        if (currentRole == Role.Photographer.ToString() && targetRole != Role.Agent.ToString())
            return ApiResponse<object?>.Fail("Photographers can only delete agents.", "403");
        targetUser.IsDeleted = true; // Soft delete
        IdentityResult result = await _userManager.UpdateAsync(targetUser);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return ApiResponse<object?>.Fail($"User deletion failed: {errors}", "500");
        }
        return ApiResponse<object?>.Success(targetUser, "User deleted successfully.");


        // await using var transaction = await _context.Database.BeginTransactionAsync();
        // try
        // {
        //     IdentityResult result = await _userManager.DeleteAsync(targetUser);
        //     if (!result.Succeeded)
        //     {
        //         var errors = string.Join("; ", result.Errors.Select(e => e.Description));
        //         return ApiResponse<object?>.Fail($"User deletion failed: {errors}", "500");
        //     }
        //     await _authRepository.DeleteUserProfileAsync(targetUserId, Enum.Parse<Role>(roles.FirstOrDefault() ?? string.Empty));
        //     await transaction.CommitAsync();
        //     return ApiResponse<object?>.Success(targetUser, "User deleted successfully.");

        // }
        // catch (System.Exception ex)
        // {
        //     await transaction.RollbackAsync();
        //     return ApiResponse<object?>.Fail($"Internal server error: {ex.Message}");
        // }

    }
    
 
    }
