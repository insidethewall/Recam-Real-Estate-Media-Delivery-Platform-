using RecamSystemApi.Models;
using RecamSystemApi.Utility;

public interface IAgentListingCaseValidator
{
    Task<ApiResponse<object?>> ValidateAgentAndListingCaseAsync(string agentId, string listingCaseId);
    Task<ApiResponse<Agent?>> ValidateAgentAsync( string agentId);
    Task<ApiResponse<ListingCase?>> ValidateListingCaseAsync(string listingCaseId);
}