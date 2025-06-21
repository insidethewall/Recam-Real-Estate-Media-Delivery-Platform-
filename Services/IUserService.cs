using RecamSystemApi.DTOs;
using RecamSystemApi.Utility;

public interface IUserService
{
    Task<string> CreateAgentAsync(AgentCreateDto registerRequest, string currentUserId);
    Task<ApiResponse<object?>> DeleteUserAsync(string currentUserId, string targetUserId);
    Task<ApiResponse<object?>> AddAgentAsync(string agentEmail, string currentUserId);
    Task<ICollection<Agent>> GetAllAgentsAsync();
    Task<ICollection<Photographer>> GetAllPhotographersAsync();
    Task<ICollection<AgentInfoDto>> GetAgentsByPhotographerAsync(string photographerId);
}