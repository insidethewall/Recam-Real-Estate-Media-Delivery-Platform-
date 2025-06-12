using RecamSystemApi.Models;

namespace RecamSystemApi.Services;

public interface IListingCasesService
{
    Task<ICollection<ListingCase>> GetAllListingCasesAsync();

}