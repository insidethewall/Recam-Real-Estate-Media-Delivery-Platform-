using RecamSystemApi.DTOs;
using RecamSystemApi.Models;
using RecamSystemApi.Utility;

public interface IUserService
{
    Task<string> CreateAgentAsync(AgentCreateDto registerRequest, string currentUserId);
    Task<ApiResponse<object?>> DeleteUserAsync(string currentUserId, string targetUserId);
    Task<AgentPhotographer> AddAgentAsync(User currentUser, User agentUser);
    Task<ICollection<UserInfoDto>> GetAllAgentsAsync();
    Task<ICollection<UserInfoDto>> GetAllPhotographersAsync();
    Task<ICollection<UserInfoDto>> GetAgentsByPhotographerAsync(string photographerId);
}