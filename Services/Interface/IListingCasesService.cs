using RecamSystemApi.DTOs;
using RecamSystemApi.Models;
using RecamSystemApi.Utility;

namespace RecamSystemApi.Services;

public interface IListingCasesService
{
    Task<ICollection<ListingCase>> GetAllListingCasesAsync();
    Task<ApiResponse<object?>> CreateListingCaseAsync(ListingCaseDto listingCaseDto, string currentUserId);
    Task<ApiResponse<object?>> AddAgentsToListingCaseAsync(ICollection<string> agentIds, string listingCaseId);

    Task<ApiResponse<UpdateListingCaseDto?>> UpdateListingCaseAsync(UpdateListingCaseDto listingCaseDto, string listingCaseId);
    Task<ApiResponse<ListingCaseStatusDto?>> ChangeListingCaseStatusAsync(ListcaseStatus newStatus, string listingCaseId);
    Task<ApiResponse<ICollection<ListingCase>>> GetAllListingCasesByAgentAsync(string currentUserId);
    Task<ApiResponse<ICollection<ListingCase>>> GetAllListingCasesByCreatorAsync(string currentUserId);

}