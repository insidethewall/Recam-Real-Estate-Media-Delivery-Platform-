using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using RecamSystemApi.DTOs;
using RecamSystemAPI.Services;

namespace RecamSystemAPI.Controllers
{
    /// <summary>
    /// AUTH API
    /// </summary>
    [Microsoft.AspNetCore.Components.Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult>  Register([FromBody] RegisterRequestDto registerRequest)
        {
            string token = await _authService.Register(registerRequest);
            return StatusCode(201, token);
        }
    }
}