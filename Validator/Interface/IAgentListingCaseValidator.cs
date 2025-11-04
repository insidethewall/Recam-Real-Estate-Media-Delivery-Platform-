using RecamSystemApi.Enums;
using RecamSystemApi.Models;
using RecamSystemApi.Utility;

public interface IAgentListingCaseValidator
{
    Task<Role> GetRole(User user);
    Task<bool> ValidateAgentAndListingCaseAsync(string agentId, string listingCaseId);
    Task<User> ValidateUserByRoleAsync( string userId, Role role);
    Task<ListingCase> ValidateListingCaseAsync(string listingCaseId);
}