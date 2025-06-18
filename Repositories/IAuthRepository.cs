using RecamSystemApi.Enums;
using RecamSystemApi.Models;
using RecamSystemApi.Utility;

public interface IAuthRepository
{
    public Task CreatePhotographerAsync(IUserProfileDto userProfile, User user);
    public Task CreateAgentAsync(IUserProfileDto userProfile, User user);
    public Task CreateAgentPhotographerAsync(string photographerId, string agentId);
  
}