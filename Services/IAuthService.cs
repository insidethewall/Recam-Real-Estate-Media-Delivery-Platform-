
using RecamSystemApi.DTOs;
using RecamSystemApi.Utility;

namespace RecamSystemApi.Services;

public interface IAuthService
{
    Task<string> Register(RegisterRequestDto registerRequest);
    Task<string> Login(LoginRequestDto loginRequest);
    Task<string> CreateAgentAsync(AgentCreateDto registerRequest, string currentUserId);
    Task<ApiResponse<object?>> DeleteUserAsync(string currentUserId, string targetUserId);
    Task<ApiResponse<object?>> AddAgentAsync(string agentEmail, string currentUserId);
    }
