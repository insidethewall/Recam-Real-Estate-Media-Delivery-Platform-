using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecamSystemApi.DTOs;
using RecamSystemApi.Enums;
using RecamSystemApi.Services;

namespace RecamSystemApi.Controllers
{
    /// <summary>
    /// AUTH API
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequest)
        {
            if (registerRequest.Role == Enums.Role.Admin || registerRequest.Role == Role.Photographer)
            {
                string token = await _authService.Register(registerRequest);
                return StatusCode(201, token);
            }
            else
            {
                return BadRequest("Role must be Admin, Photographer or Agent. Agents can only be created by Admin or Photographer.");
            }

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
            string token = await _authService.CreateAgentAsync(registerRequest, currentUserId);
            return StatusCode(201, token);

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            string token = await _authService.Login(loginRequest);
            return Ok(token);
        }

        [Authorize(Roles = "Admin, Photographer")]
        [HttpDelete("deleteUser")]
        public async Task<IActionResult> DeleteUser([FromQuery] string userId)
        {
            string? currentUserId = User.FindFirst("UserId")?.Value;
            Console.WriteLine($"Current User ID: {currentUserId}");
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("Current user ID not found.");
            }

            var response = await _authService.DeleteUserAsync(currentUserId, userId);
            return response.Succeed
                ? Ok(response.Data)
                : BadRequest(response.ErrorMessage);
        }
    }
}