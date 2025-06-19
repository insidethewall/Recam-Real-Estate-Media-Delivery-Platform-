using RecamSystemApi.Models;

public interface IUserRepository
{
    public Task CreateAgentAsync(IUserProfileDto userProfile, User user);
    public Task CreateAgentPhotographerAsync(string photographerId, string agentId);
    public Task DeleteAgentPhotographerCompany(string userId);
}