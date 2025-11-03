using RecamSystemApi.Models;

public interface IListingCasesLogRepository
{

    Task<ListingCaseLog> CreateListingCaseLog(ListingCase listingCase,ChangeType changeType, User creator, User? changer = null, string? info = null, List<FieldChange>? changes = null );
    Task AddLog(ListingCaseLog log);
    Task<ICollection<ListingCaseLog>> GetAllListingCasesLog();
    Task<ICollection<ListingCaseLog>> GetLogsByListingCaseId(string id);
    Task<ICollection<ListingCaseLog>> GetLogsByCreatorId(string id);
    Task<ICollection<ListingCaseLog>> GetLogsByChangerId(string id);
    
}