using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RecamSystemApi.DTOs;
using RecamSystemApi.Enums;
using RecamSystemApi.Models;


namespace RecamSystemApi.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuthRepository _authRepository;

    private readonly IMapper _mapper;


    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="userManager"></param>
    public AuthService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IJwtTokenService jwtTokenService, IAuthRepository authRepository, IMapper mapper)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenService = jwtTokenService;
        _authRepository = authRepository;
        _mapper = mapper;

    }

    public async Task<string> Register(RegisterRequestDto registerRequest)
    {
        User user = new User
        {
            Email = registerRequest.Email,
            UserName = registerRequest.Email,
            CreatedAt = DateTime.UtcNow,
        };
        Console.WriteLine($"UserName: {user.UserName}");
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

        switch (registerRequest.Role)
        {
            case Role.Agent:
                AgentDto agentDto = new AgentDto
                {
                    CompanyName = registerRequest.CompanyName,
                    FirstName = registerRequest.FirstName,
                    LastName = registerRequest.LastName,
                    AvatarUrl = registerRequest.AvatarUrl
                };
                await _authRepository.AddUserProfileAsync(agentDto, user);
                break;
            case Role.Photographer:
                PhotographerDto pho = new PhotographerDto
                {
                    FirstName = registerRequest.FirstName,
                    LastName = registerRequest.LastName,
                    AvatarUrl = registerRequest.AvatarUrl
                };
                await _authRepository.AddUserProfileAsync(pho, user);
                break;
            case Role.Admin:
                // Admin profile handling can be added here if needed
                break;
            default:
                throw new System.Exception("Invalid role specified.");
        }
        string token = await _jwtTokenService.GenerateTokenAsync(user);
        return token;
    }

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

    
 
    }