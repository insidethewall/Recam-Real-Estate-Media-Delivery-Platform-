using RecamSystemApi.Models;

public interface IUserLogRepository
{

    UserLog CreateUserLog(User user, UserAction action, User? targetUser, string? AdditionalInfo);
    Task AddLog(UserLog log);
    Task DeleteLog(UserLog log);
    Task<ICollection<UserLog>> GetAllUsersLog();
    Task<ICollection<UserLog>> GetLogsByUserId(string id);
    
}