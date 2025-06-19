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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            string token = await _authService.Login(loginRequest);
            return Ok(token);
        }


    }
}