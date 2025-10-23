using RecamSystemApi.DTOs;
using RecamSystemApi.Models;
using RecamSystemApi.Utility;

namespace RecamSystemApi.Services;

public interface IListingCasesService
{
    Task<ICollection<ListingCase>> GetAllListingCasesAsync();
    Task<ListingCaseDto> CreateListingCaseAsync(ListingCaseDto listingCaseDto, User currentUser);
     Task<UpdateListingCaseDto> UpdateListingCaseAsync(UpdateListingCaseDto listingCaseDto, string listingCaseId);
    Task<ListingCaseStatusDto> ChangeListingCaseStatusAsync(ListcaseStatus newStatus, string listingCaseId);
    Task<List<AgentListingCase>> AddAgentsToListingCaseAsync(ICollection<string> agentIds, string listingCaseId);
    Task<ICollection<ListingCase>> GetAllListingCasesByAgentAsync(string currentUserId);
    Task<ICollection<ListingCase>> GetAllListingCasesByCreatorAsync(User currentUser);
    Task<ListingCase> DeleteListingCaseAsync(string listingCaseId);

}