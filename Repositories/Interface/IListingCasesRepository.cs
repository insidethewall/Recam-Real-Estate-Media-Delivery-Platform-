using RecamSystemApi.DTOs;
using RecamSystemApi.Models;
using RecamSystemApi.Utility;

public interface IListingCasesRepository
{
    Task<ICollection<ListingCase>> GetAllListingCasesAsync();
    Task<ApiResponse<object?>> CreateListingCaseAsync(ListingCaseDto listingCaseDto, User user);
    // Task<ApiResponse<object?>> CreateAgentListingCaseAsync(Agent agent, ListingCase listingCase);
    Task<ListingCase> GetListingCaseByIdAsync(string listingCaseId);
    Task<ICollection<ListingCase>> GetAllListingCasesByUserAsync(User currentUser);
    ICollection<ListingCase> GetAllListingCasesByAgentAsync(Agent agent);
    // Task<ApiResponse<UpdateListingCaseDto?>> UpdateListingCaseAsync(UpdateListingCaseDto listingCaseDto, string listingCaseId);
  
    void DeleteListingCase(ListingCase listingCase);
    Task AddAgentListingCaseAsync(AgentListingCase agentListingCase);
    Task AddListingCaseAsync(ListingCase listingCase);
}

 