using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecamSystemApi.Data;
using RecamSystemApi.Enums;
using RecamSystemApi.Models;
using RecamSystemApi.Utility;


public class AgentListingCaseValidator : IAgentListingCaseValidator
{
    private readonly ReacmDbContext _context;
    private readonly UserManager<User> _userManager;

   

    public AgentListingCaseValidator(ReacmDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
      
    }

    public async Task<ApiResponse<object?>> ValidateAgentAndListingCaseAsync(string agentId, string listingCaseId)
    {
        var exists = await _context.AgentListingCases
            .AnyAsync(alc => alc.AgentId == agentId && alc.ListingCaseId == listingCaseId);

        if (exists)
            return ApiResponse<object?>.Fail($"Agent with ID {agentId} is already added to the listing case.", "400");
        return ApiResponse<object?>.Success(null, "Agent and listing case validation successful.");
    }

    public async Task<ApiResponse<Agent?>> ValidateAgentAsync(string agentId)
    {
        var user = await _userManager.FindByIdAsync(agentId);
        if (user == null)
            return ApiResponse<Agent?>.Fail($"Agent with ID {agentId} not found.", "404");

        if (user.IsDeleted)
            return ApiResponse<Agent?>.Fail($"Agent with ID {agentId} is deleted.", "403");

        var roles = await _userManager.GetRolesAsync(user);
        var userRole = roles.FirstOrDefault();
        if (userRole != Role.Agent.ToString())
            return ApiResponse<Agent?>.Fail($"User with ID {agentId} is not an agent.", "403");

        var agent = user.Agent;
        if (agent == null)
            return ApiResponse<Agent?>.Fail($"User with ID {agentId} does not have an associated Agent entity.", "403");
        return ApiResponse<Agent?>.Success(agent);
    }

    public async Task<ApiResponse<ListingCase?>> ValidateListingCaseAsync(string listingCaseId)
    {
        var listingCase = await _context.ListingCases.FindAsync(listingCaseId);
        if (listingCase == null)
            return ApiResponse<ListingCase?>.Fail($"Listing case with ID {listingCaseId} not found.", "404");

        if (listingCase.IsDeleted)
            return ApiResponse<ListingCase?>.Fail($"Listing case with ID {listingCaseId} is deleted.", "403");

        return ApiResponse<ListingCase?>.Success(listingCase);
    }
}