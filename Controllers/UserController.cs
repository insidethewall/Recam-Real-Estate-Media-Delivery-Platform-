using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecamSystemApi.DTOs;

namespace RecamSystemApi.Controllers;

[Route("api/[controller]")]
[ApiController]

public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    // Endpoint to register an agent, only accessible by Admin
    [Authorize(Roles = "Admin")]
    [HttpPost("registerAgent")]
    public async Task<IActionResult> RegisterAgent([FromBody] AgentCreateDto registerRequest)
    {
        string? currentUserId = User.FindFirst("UserId")?.Value;
        Console.WriteLine($"Current User ID: {currentUserId}");
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized("Current user ID not found.");
        }
        string token = await _userService.CreateAgentAsync(registerRequest, currentUserId);
        return StatusCode(201, token);

    }

    [Authorize(Roles = "Photographer")]
    [HttpPost("addAgent")]

    public async Task<IActionResult> AddAgent([FromBody] string agentEmail)
    {
        string? currentUserId = User.FindFirst("UserId")?.Value;
        Console.WriteLine($"Current User ID: {currentUserId}");
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized("Current user ID not found.");
        }

        var response = await _userService.AddAgentAsync(agentEmail, currentUserId);
        return response.Succeed
            ? Ok(response.Data)
            : BadRequest(response.ErrorMessage);

    }
        
    // Endpoint to delete a user, only accessible by Admin
    [Authorize(Roles = "Admin")]
    [HttpDelete("deleteUser")]
    public async Task<IActionResult> DeleteUser([FromQuery] string userId)
    {
        string? currentUserId = User.FindFirst("UserId")?.Value;
        Console.WriteLine($"Current User ID: {currentUserId}");
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized("Current user ID not found.");
        }

        var response = await _userService.DeleteUserAsync(currentUserId, userId);
        return response.Succeed
            ? Ok(response.Data)
            : BadRequest(response.ErrorMessage);
    }
    

    
}