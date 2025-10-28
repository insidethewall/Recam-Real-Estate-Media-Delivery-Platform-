using RecamSystemApi.DTOs;
using RecamSystemApi.Models;

public interface IUserRepository
{
    public Task CreateAgentAsync(Agent userProfile, User user);
    public Task<AgentPhotographer> CreateAgentPhotographerAsync(User currentUser, User agentUser);
    public Task<ICollection<AgentPhotographer>> DeleteAgentPhotographerCompany(User user);

    public Task<ICollection<UserInfoDto>> GetAllAgentsAsync();

    public Task<ICollection<UserInfoDto>> GetAllPhotographersAsync();

    public Task<ICollection<UserInfoDto>> GetAgentsByPhotographerAsync(string photographerId);
    public Photographer DeletePhotographer(User user);

    public Agent DeleteAgent(User user);
}