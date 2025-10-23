using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecamSystemApi.Data;
using RecamSystemApi.Enums;
using RecamSystemApi.Exception;
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

    public async Task ValidateAgentAndListingCaseAsync(string agentId, string listingCaseId)
    {
        var exists = await _context.AgentListingCases
            .AnyAsync(alc => alc.AgentId == agentId && alc.ListingCaseId == listingCaseId);

        if (exists)
            throw new Exception($"Agent with ID {agentId} is already associated with listing case ID {listingCaseId}.");
       
    }

    public async Task<User> ValidateUserByRoleAsync(string userId, Role role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException($"User with ID {userId} not found.");

        if (user.IsDeleted)
            throw new Exception($"User with ID {userId} is deleted.");

        var roles = await _userManager.GetRolesAsync(user);
        var userRole = roles.FirstOrDefault();
        if (userRole != role.ToString())
            throw new Exception($"User with ID {userId} is not {role}.");
        if (role == Role.Agent)
        {
            Agent? agent = user.Agent;
            if (agent == null)
                throw new Exception($"Agent profile for user ID {userId} not found.");
        }
        if (role == Role.Photographer)
        {
            Photographer? photographer = user.Photographer;
            if (photographer == null)
                throw new Exception($"Photographer profile for user ID {userId} not found.");
        }
        return user;
           
       
       
    }

    public async Task<ListingCase> ValidateListingCaseAsync(string listingCaseId)
    {
        var listingCase = await _context.ListingCases.FindAsync(listingCaseId);
        if (listingCase == null)
           throw new Exception($"Listing case with ID {listingCaseId} not found.");

        if (listingCase.IsDeleted)
            throw new Exception($"Listing case with ID {listingCaseId} is deleted.");

        return listingCase;
    }
}