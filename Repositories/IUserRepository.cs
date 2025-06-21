using RecamSystemApi.DTOs;
using RecamSystemApi.Models;

public interface IUserRepository
{
    public Task CreateAgentAsync(IUserProfileDto userProfile, User user);
    public Task CreateAgentPhotographerAsync(string photographerId, string agentId);
    public Task DeleteAgentPhotographerCompany(string userId);

    public Task<ICollection<Agent>> GetAllAgentsAsync();

    public Task<ICollection<Photographer>> GetAllPhotographersAsync();

     public Task<ICollection<AgentInfoDto>> GetAgentsByPhotographerAsync(string photographerId);
}