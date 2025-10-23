using Microsoft.AspNetCore.Identity;
using RecamSystemApi.Data;
using RecamSystemApi.DTOs;
using RecamSystemApi.Enums;
using RecamSystemApi.Models;


namespace RecamSystemApi.Services;

public class AuthService : IAuthService
{
    private readonly ReacmDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuthRepository _authRepository;



    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="userManager"></param>
    public AuthService(ReacmDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IJwtTokenService jwtTokenService, IAuthRepository authRepository)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenService = jwtTokenService;
        _authRepository = authRepository;
    }

    // only for Photographer roles
    public async Task<string> Register(RegisterRequestDto registerRequest)
    {
        User user = new User
        {
            Email = registerRequest.Email,
            UserName = registerRequest.Email,
            CreatedAt = DateTime.UtcNow,
        };
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var roleName = Role.Photographer.ToString();

            IdentityResult result = await _userManager.CreateAsync(user, registerRequest.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new UserRegistrationException($"User creation failed: {errors}");
            }

            await _userManager.AddToRoleAsync(user, roleName);
            await _authRepository.CreatePhotographerAsync(registerRequest, user);
            await transaction.CommitAsync();
            string token = await _jwtTokenService.GenerateTokenAsync(user);
            return token;
        }
        catch (System.Exception ex)
        {
            await transaction.RollbackAsync();
            await _userManager.DeleteAsync(user); // Clean up in case of error
            throw new System.Exception($"Internal server error: {ex.Message}");
        }

    }

    // login for all roles
    public async Task<string> Login(LoginRequestDto loginRequest)
    {
        User? user = await _userManager.FindByEmailAsync(loginRequest.Email);
        if (user == null)
            throw new System.Exception("The user does not exist.");
        if (user.IsDeleted)
            throw new System.Exception("User account is deleted or inactive.");
        bool isPasswordValid = await _userManager.CheckPasswordAsync(user, loginRequest.Password);
        if (!isPasswordValid)
            throw new System.Exception("Invalid password.");

        return await _jwtTokenService.GenerateTokenAsync(user);
    }    
 
}
