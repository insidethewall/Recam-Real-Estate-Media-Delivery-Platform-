using RecamSystemApi.DTOs;
using RecamSystemApi.Models;
using RecamSystemApi.Utility;

public interface IListingCasesRepository
{
    Task<ICollection<ListingCase>> GetAllListingCasesAsync();
    Task<ApiResponse<object?>> CreateListingCaseAsync(ListingCaseDto listingCaseDto, User user);
    Task<ApiResponse<object?>> CreateAgentListingCaseAsync(Agent agent, ListingCase listingCase);
    Task<ApiResponse<ICollection<ListingCase>>> GetAllListingCasesByUserAsync(User currentUser);
    ApiResponse<ICollection<ListingCase>> GetAllListingCasesByAgentAsync(Agent agent);
    Task<ApiResponse<UpdateListingCaseDto?>> UpdateListingCaseAsync(UpdateListingCaseDto listingCaseDto, string listingCaseId);
   Task<ApiResponse<ListingCaseStatusDto?>> ChangeListingCaseStatusAsync(ListcaseStatus status, ListingCase listingCase);
}

 