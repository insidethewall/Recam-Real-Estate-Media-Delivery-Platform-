using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecamSystemApi.Data;
using RecamSystemApi.DTOs;
using RecamSystemApi.Enums;
using RecamSystemApi.Models;
using RecamSystemApi.Services;
using RecamSystemApi.Utility;

public class ListingCasesService : IListingCasesService
{
    private readonly IListingCasesRepository _repository;
    private readonly UserManager<User> _userManager;

    private readonly ReacmDbContext _context;
     private readonly IAgentListingCaseValidator _validator;
    public ListingCasesService(IListingCasesRepository repository, UserManager<User> userManager, ReacmDbContext context, IAgentListingCaseValidator validator)
    {
        _repository = repository;
        _userManager = userManager;
        _context = context;
        _validator = validator;

    }

    public async Task<ICollection<ListingCase>> GetAllListingCasesAsync()
    {
        return await _repository.GetAllListingCasesAsync();
    }

    public async Task<ApiResponse<object?>> CreateListingCaseAsync(ListingCaseDto listingCaseDto, string currentUserId)
    {
        User? currentUser = await _userManager.FindByIdAsync(currentUserId);
        if (currentUser == null)
        {
            return ApiResponse<object?>.Fail("Current user not found.", "404");
        }
        if (listingCaseDto == null)
        {
            return ApiResponse<object?>.Fail("Listing case cannot be null.", "400");
        }
        try
        {

            await _repository.CreateListingCaseAsync(listingCaseDto, currentUser);
            return ApiResponse<object?>.Success(listingCaseDto, "Listing case created successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse<object?>.Fail($"Error creating listing case: {ex.Message}", "500");
        }
    }
    
    public async Task<ApiResponse<object?>> AddAgentsToListingCaseAsync(ICollection<string> agentIds, string listingCaseId)
    {
            
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            ApiResponse<ListingCase?> listingCaseResponse = await _validator.ValidateListingCaseAsync(listingCaseId);
            if (!listingCaseResponse.Succeed)
                return ApiResponse<object?>.Fail(listingCaseResponse.Message ?? "listing case Unknown error.", listingCaseResponse.ErrorCode);

            ListingCase listingCase = listingCaseResponse.Data!;
            var agentListingCases = new List<AgentListingCase>();

            foreach (string agentId in agentIds)
            {
                 var agentResult = await _validator.ValidateAgentAsync(agentId, listingCaseId);
                if (!agentResult.Succeed)
                    return ApiResponse<object?>.Fail(agentResult.Message ?? "listing case Unknown error.", agentResult.ErrorCode);

                var agent = agentResult.Data!;
                await _repository.CreateAgentListingCaseAsync(agent, listingCase);

            }
            return ApiResponse<object?>.Success(null, "Agents added to listing case successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse<object?>.Fail($"Error adding agents to listing case: {ex.Message}", "500");
        }
    }

}