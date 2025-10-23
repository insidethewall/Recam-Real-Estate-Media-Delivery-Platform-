using Microsoft.AspNetCore.Mvc;
using RecamSystemApi.DTOs;
using RecamSystemApi.Services;
using RecamSystemApi.Utility;

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
        private readonly ILogger<AuthController> _logger;
        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequest)
        {
            try
            { 
                string token = await _authService.Register(registerRequest);
                return StatusCode(201, ApiResponse<string>.Success(token, "User registered successfully."));
                
            } catch (UserRegistrationException ex)
            {
                _logger.LogError(ex, "User registration failed.");
                return BadRequest(ApiResponse<string>.Fail(ex.Message, "400"));
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during user registration.");
                return StatusCode(500, ApiResponse<string>.Fail(ex.Message, "500"));
            }
          
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            string token = await _authService.Login(loginRequest);
            return Ok(ApiResponse<string>.Success(token, "User logged in successfully."));
        }


    }
}