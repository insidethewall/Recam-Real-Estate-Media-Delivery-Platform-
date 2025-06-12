using RecamSystemApi.Models;

public interface IListingCasesRepository
{ 
    Task<ICollection<ListingCase>> GetAllListingCasesAsync();
}