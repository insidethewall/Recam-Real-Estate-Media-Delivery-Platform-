using RecamSystemApi.Enums;
using RecamSystemApi.Models;
using RecamSystemApi.Utility;

public interface IAgentListingCaseValidator
{
    Task ValidateAgentAndListingCaseAsync(string agentId, string listingCaseId);
    Task<User> ValidateUserByRoleAsync( string userId, Role role);
    Task<ListingCase> ValidateListingCaseAsync(string listingCaseId);
}