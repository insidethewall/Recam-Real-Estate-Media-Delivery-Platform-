using RecamSystemApi.Models;
using RecamSystemApi.Utility;

public interface IAgentListingCaseValidator
{
    Task<ApiResponse<Agent?>> ValidateAgentAsync(string agentId, string listingCaseId);
    Task<ApiResponse<ListingCase?>> ValidateListingCaseAsync(string listingCaseId);
}