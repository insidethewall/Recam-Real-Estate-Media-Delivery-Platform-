


using MongoDB.Driver;
using RecamSystemApi.DTOs;
using RecamSystemApi.Models;

public class ListingCasesLogRepository : IListingCasesLogRepository
{
     private readonly MongoDbContext _mongoDbContext;
    private readonly IGeneralRepository _generalRepository;

    private readonly IAgentListingCaseValidator _validator;

    public ListingCasesLogRepository(MongoDbContext mongoDbContext, IGeneralRepository generalRepository, IAgentListingCaseValidator validator)
    {
        _mongoDbContext = mongoDbContext;
        _generalRepository = generalRepository;
        _validator = validator;
    }

    public async Task<ListingCaseLog> CreateListingCaseLog(ListingCase listingCase, ChangeType changeType, User creator, User? changer, string? info , List<FieldChange>? changes)
    {
        ListingCaseLog listingCaseLog = _generalRepository.MapDto<ListingCase, ListingCaseLog>(listingCase);
        listingCaseLog.ChangeType = changeType;
        UserDetailDto createdBy = _generalRepository.MapDto<User, UserDetailDto>(creator);
        
        createdBy.Role =  await _validator.GetRole(creator);
        if (changer != null)
        {
            UserDetailDto changedBy = _generalRepository.MapDto<User, UserDetailDto>(changer);
             changedBy.Role = await _validator.GetRole(changer);
        }
        listingCaseLog.Changes = changes ?? new List<FieldChange>();
       
        listingCaseLog.CreatedBy = createdBy;
        listingCaseLog.AdditionalInfo = info ?? "";
        return listingCaseLog;
    
    }


    public async Task AddLog(ListingCaseLog log)
    {
        await _mongoDbContext.ListingCaseLogs.InsertOneAsync(log);

    }

    public async Task<ICollection<ListingCaseLog>> GetAllListingCasesLog()
    {
        return await _mongoDbContext.ListingCaseLogs.Find(_ => true).SortByDescending(doc=>doc.TimeStamp).ToListAsync();
    }

    public async Task<ICollection<ListingCaseLog>> GetLogsByChangerId(string id)
    {
        return await _mongoDbContext.ListingCaseLogs
            .Find(doc => doc.ChangedBy != null && doc.ChangedBy.UserId == id)
            .SortByDescending(doc => doc.TimeStamp)
            .ToListAsync();
    }

    public async Task<ICollection<ListingCaseLog>> GetLogsByCreatorId(string id)
    {
           return await _mongoDbContext.ListingCaseLogs
            .Find(doc => doc.CreatedBy != null && doc.CreatedBy.UserId == id)
            .SortByDescending(doc => doc.TimeStamp)
            .ToListAsync();
    }

    public async Task<ICollection<ListingCaseLog>> GetLogsByListingCaseId(string id)
    {
        return await _mongoDbContext.ListingCaseLogs
            .Find(doc => doc.ListingCaseId == id)
            .SortByDescending(doc => doc.TimeStamp)
            .ToListAsync();
    }
}