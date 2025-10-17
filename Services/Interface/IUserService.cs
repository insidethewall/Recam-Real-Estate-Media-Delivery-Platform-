using RecamSystemApi.DTOs;
using RecamSystemApi.Utility;

public interface IUserService
{
    Task<string> CreateAgentAsync(AgentCreateDto registerRequest, string currentUserId);
    Task<ApiResponse<object?>> DeleteUserAsync(string currentUserId, string targetUserId);
    Task<ApiResponse<object?>> AddAgentAsync(string agentEmail, string currentUserId);
    Task<ICollection<UserInfoDto>> GetAllAgentsAsync();
    Task<ICollection<UserInfoDto>> GetAllPhotographersAsync();
    Task<ICollection<UserInfoDto>> GetAgentsByPhotographerAsync(string photographerId);
}