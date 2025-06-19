using RecamSystemApi.DTOs;
using RecamSystemApi.Utility;

public interface IUserService
{
    Task<string> CreateAgentAsync(AgentCreateDto registerRequest, string currentUserId);
    Task<ApiResponse<object?>> DeleteUserAsync(string currentUserId, string targetUserId);
    Task<ApiResponse<object?>> AddAgentAsync(string agentEmail, string currentUserId);
}