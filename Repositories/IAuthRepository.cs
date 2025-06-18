using RecamSystemApi.Enums;
using RecamSystemApi.Models;
using RecamSystemApi.Utility;

public interface IAuthRepository
{
    public Task AddUserProfileAsync(Role role, IUserProfileDto userProfile, User user);
  
}