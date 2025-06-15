using RecamSystemApi.Models;

public interface IAuthRepository
{
    public Task AddUserProfileAsync(IUserProfileDto userProfile, User user);
}