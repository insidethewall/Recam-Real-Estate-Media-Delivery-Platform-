using RecamSystemApi.DTOs;
using RecamSystemApi.Models;
using RecamSystemApi.Utility;

public interface IListingCasesRepository
{
    Task<ICollection<ListingCase>> GetAllListingCasesAsync();

    Task<ApiResponse<object?>> CreateListingCaseAsync(ListingCaseDto listingCaseDto, User user);
    Task<ApiResponse<object?>> CreateAgentListingCaseAsync(Agent agent, ListingCase listingCase);
}

 