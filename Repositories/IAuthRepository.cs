using RecamSystemApi.Enums;
using RecamSystemApi.Models;
using RecamSystemApi.Utility;

public interface IAuthRepository
{
    public Task AddPhotographerAsync(IUserProfileDto userProfile, User user);
    public Task AddAgentAsync(IUserProfileDto userProfile, User user);
    public Task AddAgentPhotographerAsync(string photographerId, string agentId);
  
}