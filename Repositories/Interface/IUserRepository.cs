using RecamSystemApi.DTOs;
using RecamSystemApi.Models;

public interface IUserRepository
{
    public Task CreateAgentAsync(IUserProfileDto userProfile, User user);
    public Task CreateAgentPhotographerAsync(string photographerId, string agentId);
    public Task DeleteAgentPhotographerCompany(string userId);

    public Task<ICollection<UserInfoDto>> GetAllAgentsAsync();

    public Task<ICollection<UserInfoDto>> GetAllPhotographersAsync();

     public Task<ICollection<UserInfoDto>> GetAgentsByPhotographerAsync(string photographerId);
}