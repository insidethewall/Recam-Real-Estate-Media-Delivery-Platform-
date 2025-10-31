using RecamSystemApi.DTOs;
using RecamSystemApi.Models;
using RecamSystemApi.Utility;

public interface IListingCasesRepository
{
    Task<ICollection<ListingCaseWithNavDto>> GetAllListingCasesAsync();

    Task<ICollection<ListingCaseWithNavDto>> GetAllDeletedListingCasesAsync();
    Task<ApiResponse<object?>> CreateListingCaseAsync(ListingCaseDto listingCaseDto, User user);
    // Task<ApiResponse<object?>> CreateAgentListingCaseAsync(Agent agent, ListingCase listingCase);
    Task<ListingCase> GetListingCaseByIdAsync(string listingCaseId);
    Task<ICollection<ListingCase>> GetAllListingCasesByUserAsync(User currentUser);
    ICollection<ListingCase> GetAllListingCasesByAgentAsync(Agent agent);
    // Task<ApiResponse<UpdateListingCaseDto?>> UpdateListingCaseAsync(UpdateListingCaseDto listingCaseDto, string listingCaseId);
    void DeleteAgentListingCase(ListingCase listingCase);
    void SoftDeleteListingCase(ListingCase listingCase);

    void SoftDeleteMediaAssetsByListingCase(ListingCase listingCase);
    void RemoveListingCaseFromUser(ListingCase listingCase);
    Task AddAgentListingCaseAsync(AgentListingCase agentListingCase);
    Task AddListingCaseAsync(ListingCase listingCase);
}

 