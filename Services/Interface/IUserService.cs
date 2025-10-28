using RecamSystemApi.DTOs;
using RecamSystemApi.Models;
using RecamSystemApi.Utility;

public interface IUserService
{
    Task<string> CreateAgentAsync(AgentCreateDto registerRequest, string currentUserId);
    Task<AgentPhotographer> AddAgentAsync(User currentUser, User agentUser);
    Task<UserDeletionDto> DeleteUserAsync(string currentUserId, string targetUserId);
    Task<ICollection<UserInfoDto>> GetAllAgentsAsync();
    Task<ICollection<UserInfoDto>> GetAllPhotographersAsync();
    Task<ICollection<UserInfoDto>> GetAgentsByPhotographerAsync(string photographerId);
}