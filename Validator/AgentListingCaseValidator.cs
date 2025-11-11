using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecamSystemApi.Data;
using RecamSystemApi.Enums;
using RecamSystemApi.Exception;
using RecamSystemApi.Models;



public class AgentListingCaseValidator : IAgentListingCaseValidator
{
    private readonly ReacmDbContext _context;
    private readonly UserManager<User> _userManager;

   

    public AgentListingCaseValidator(ReacmDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
      
    }

    public async Task<bool> ValidateAgentAndListingCaseAsync(string agentId, string listingCaseId)
    {
        bool exists = await _context.AgentListingCases
            .AnyAsync(alc => alc.AgentId == agentId && alc.ListingCaseId == listingCaseId);
        return exists;
       
       
    }

    // public async Task<User> ValidateUser(string userId)
    // {
    //     var user = await _userManager.FindByIdAsync(userId);
    //     if (user == null)
    //         throw new NotFoundException($"User with ID {userId} not found.");

    //     if (user.IsDeleted)
    //         throw new Exception($"User with ID {userId} is deleted.");

    //     return user;
    // }
    public async Task<Role> GetRole(User user)
    { 
        var roles = await _userManager.GetRolesAsync(user);
        var userRole = roles.FirstOrDefault();
        if (userRole == null)
            throw new Exception($"User with ID {user.Id} has no role assigned.");

        if (!Enum.TryParse<Role>(userRole, ignoreCase: true, out var parsedRole))
            throw new Exception($"Unknown role '{userRole}' for user ID {user.Id}.");

        return parsedRole;
    }


public async Task<User> ValidateUserByRoleAsync(string userId, Role role)
{
    User? user = await _userManager.Users
        .Include(u => u.Agent)
        .Include(u => u.Photographer)
        .FirstOrDefaultAsync(u => u.Id == userId);
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
        ListingCase? listingCase = await _context.ListingCases.Include(l=>l.User).FirstOrDefaultAsync(l=>l.Id == listingCaseId);
        if (listingCase == null)
           throw new Exception($"Listing case with ID {listingCaseId} not found.");

        if (listingCase.IsDeleted)
            throw new Exception($"Listing case with ID {listingCaseId} is deleted.");

        return listingCase;
    }
}