using MongoDB.Driver;
using RecamSystemApi.DTOs;
using RecamSystemApi.Models;

public class UserLogRepository : IUserLogRepository
{
    private readonly MongoDbContext _mongoDbContext;
    private readonly IGeneralRepository _generalRepository;

    private readonly IAgentListingCaseValidator _validator;
    public UserLogRepository(MongoDbContext mongoDbContext, IGeneralRepository generalRepository, IAgentListingCaseValidator validator)
    {
        _mongoDbContext = mongoDbContext;
        _generalRepository = generalRepository;
        _validator = validator;
    }


    public UserLog CreateUserLog(User user, UserAction action, User? targetUser = null, string? AdditionalInfo = null)
    {
        UserDetailDto userDetail = _generalRepository.MapDto<User, UserDetailDto>(user);
        UserDetailDto? targetUserDetail = null;
        if (targetUser != null)
        {
            targetUserDetail = _generalRepository.MapDto<User, UserDetailDto>(targetUser);
        }
        UserLog userLog = new UserLog
        {
            UserDetail = userDetail,
            Action = action,
            TargetedUser = targetUserDetail,
            AdditionalInfo = AdditionalInfo ?? string.Empty
        };

        return userLog;
    }

    public async Task AddLog(UserLog log)
    {
        await _mongoDbContext.UserLogs.InsertOneAsync(log);
    }

    public async Task DeleteLog(UserLog log)
    {
        await _mongoDbContext.UserLogs.DeleteOneAsync(log.Id);
    }

    public async Task<ICollection<UserLog>> GetAllUsersLog()
    {
        return await _mongoDbContext.UserLogs.Find(_ => true).SortByDescending(doc => doc.TimeStamp).ToListAsync();
    }

    public async Task<ICollection<UserLog>> GetLogsByUserId(string id)
    { 
        return await _mongoDbContext.UserLogs.Find(ul=>ul.UserDetail.UserId == id).SortByDescending(doc => doc.TimeStamp)
            .ToListAsync();
        
    }
}