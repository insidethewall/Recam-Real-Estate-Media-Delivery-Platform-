using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecamSystemApi.DTOs;
using RecamSystemApi.Enums;
using RecamSystemApi.Models;
using RecamSystemApi.Utility;

namespace RecamSystemApi.Controllers;

[Route("api/[controller]")]
[ApiController]

public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly UserManager<User> _userManager;

    private readonly AgentListingCaseValidator _agentListingCaseValidator;
    public UserController(IUserService userService, UserManager<User> userManager, AgentListingCaseValidator agentListingCaseValidator)
    {
        _userService = userService;
        _userManager = userManager;
        _agentListingCaseValidator = agentListingCaseValidator;
    }
    // Endpoint to register an agent, only accessible by Admin
    [Authorize(Roles = "Admin")]
    [HttpPost("registerAgent")]
    public async Task<IActionResult> RegisterAgent([FromBody] AgentCreateDto registerRequest)
    {
        string? currentUserId = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized(ApiResponse<string>.Fail("Current user ID not found."));
        }
        string token = await _userService.CreateAgentAsync(registerRequest, currentUserId);
        return StatusCode(201, ApiResponse<string>.Success(token, "Agent registered successfully."));

    }

    [Authorize(Roles = "Photographer")]
    [HttpPost("addAgent")]

    public async Task<IActionResult> AddAgent([FromBody] string agentEmail)
    {
        string? currentUserId = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized(ApiResponse<string>.Fail("Current user ID not found.", "401"));
        }
        User currentUser = await _agentListingCaseValidator.ValidateUserByRoleAsync(currentUserId, Role.Photographer);

        User? agentUser = await _userManager.FindByEmailAsync(agentEmail);

        if (agentUser == null)
            return Unauthorized(ApiResponse<string>.Fail("Agent not found.", "401"));
        if (agentUser.IsDeleted)
        { 
            return Unauthorized(ApiResponse<string>.Fail("Agent account is deleted.", "401"));
        }

        if (!await _userManager.IsInRoleAsync(agentUser, Role.Agent.ToString()))
            return Unauthorized(ApiResponse<string>.Fail("This is not agent.", "401"));

        AgentPhotographer response = await _userService.AddAgentAsync(currentUser, agentUser);
        return Ok(ApiResponse<AgentPhotographer>.Success(response, "Agent added successfully."));

    }

    // Endpoint to delete a user, only accessible by Admin
    [Authorize(Roles = "Admin")]
    [HttpDelete("deleteUser")]
    public async Task<IActionResult> DeleteUser([FromQuery] string userId)
    {
        string? currentUserId = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized("Current user ID not found.");
        }

        var response = await _userService.DeleteUserAsync(currentUserId, userId);
        return response.Succeed
            ? Ok(response.Data)
            : BadRequest(response.ErrorMessage);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("getAgents")]
    public async Task<IActionResult> GetAllAgents()
    {
        var agents = await _userService.GetAllAgentsAsync();
        if (agents == null || !agents.Any())
        {
            return NotFound("No agents found.");
        }
        return Ok(agents);

    }

    [Authorize(Roles = "Admin")]
    [HttpGet("getPhotographers")]
    public async Task<IActionResult> GetAllPhotographers()
    {
        var Photographers = await _userService.GetAllPhotographersAsync();
        if (Photographers == null || !Photographers.Any())
        {
            return NotFound("No Photographers found.");
        }
        return Ok(Photographers);

    }

    [Authorize(Roles = "Photographer")]
    [HttpGet("getAgentsByPhotographer")]
    public async Task<IActionResult> GetAgentsByPhotographer()
    {
        string? currentUserId = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized("Current user ID not found.");
        }

        var agents = await _userService.GetAgentsByPhotographerAsync(currentUserId);
        return Ok(agents);
    }

    

    
}