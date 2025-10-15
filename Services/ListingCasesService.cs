using Microsoft.AspNetCore.Identity;
using RecamSystemApi.DTOs;
using RecamSystemApi.Models;
using RecamSystemApi.Services;
using RecamSystemApi.Utility;

public class ListingCasesService : IListingCasesService
{
    private readonly IGeneralRepository _generalRepository;
    private readonly IListingCasesRepository _repository;
    private readonly UserManager<User> _userManager;

    
    private readonly IAgentListingCaseValidator _validator;
    public ListingCasesService(IGeneralRepository generalRepository, IListingCasesRepository repository, UserManager<User> userManager, IAgentListingCaseValidator validator)
    {
        _generalRepository = generalRepository;
        _repository = repository;
        _userManager = userManager;
        _validator = validator;
    }

    public async Task<ICollection<ListingCase>> GetAllListingCasesAsync()
    {
        return await _repository.GetAllListingCasesAsync();
    }


// TODO: need to refactor 
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

        using var transaction = await _generalRepository.BeginTransactionAsync();
        try
        {
            ListingCase listingCase = _generalRepository.MapDto<ListingCaseDto, ListingCase>(listingCaseDto);
            listingCase.UserId = currentUser.Id;
            listingCase.User = currentUser;
            await _repository.AddListingCaseAsync(listingCase);
            currentUser.ListingCases.Add(listingCase);
            await _generalRepository.SaveChangesAsync();
            await transaction.CommitAsync();

            return ApiResponse<object?>.Success(listingCaseDto, "Listing case created successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ApiResponse<object?>.Fail($"Error creating listing case: {ex.Message}", "500");
        }
    }

// just basic update, no references update
    public async Task<ApiResponse<UpdateListingCaseDto?>> UpdateListingCaseAsync(UpdateListingCaseDto listingCaseDto, string listingCaseId)
    {
        try
        {
            ListingCase existingListingCase = await _repository.GetListingCaseByIdAsync(listingCaseId);
            UpdateListingCaseDto updatedDto = _generalRepository.MapDto<ListingCase, UpdateListingCaseDto>(existingListingCase);
            await _generalRepository.SaveChangesAsync();
            return ApiResponse<UpdateListingCaseDto?>.Success(listingCaseDto, "Listing case updated successfully.");
        }
        catch (InvalidOperationException)
        {
            return ApiResponse<UpdateListingCaseDto?>.Fail($"Listing case with ID {listingCaseId} not found.", "404");
        }
        catch (Exception ex)
        {
            return ApiResponse<UpdateListingCaseDto?>.Fail($"Error updating listing case: {ex.Message}", "500");
        }

    }

    public async Task<ApiResponse<ListingCaseStatusDto?>> ChangeListingCaseStatusAsync(ListcaseStatus newStatus, string listingCaseId)
    {
        try
        {
            ApiResponse<ListingCase?> listingCaseResponse = await _validator.ValidateListingCaseAsync(listingCaseId);
            if (!listingCaseResponse.Succeed)
                return ApiResponse<ListingCaseStatusDto?>.Fail(listingCaseResponse.Message ?? "listing case Unknown error.", listingCaseResponse.ErrorCode);

            ListingCase listingCase = listingCaseResponse.Data!;

            listingCase.ListcaseStatus = newStatus;

            await _generalRepository.SaveChangesAsync();
            ListingCaseStatusDto statusDto = new ListingCaseStatusDto
            {
                Id = listingCase.Id,
                title = listingCase.Title,
                Status = listingCase.ListcaseStatus
            };
            return ApiResponse<ListingCaseStatusDto?>.Success(statusDto, "Listing case status updated successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse<ListingCaseStatusDto?>.Fail($"Error updating listing case status: {ex.Message}", "500");
        }
    }

    public async Task<ApiResponse<object?>> AddAgentsToListingCaseAsync(ICollection<string> agentIds, string listingCaseId)
    {

        await using var transaction = await _generalRepository.BeginTransactionAsync();
        try
        {
            ApiResponse<ListingCase?> listingCaseResponse = await _validator.ValidateListingCaseAsync(listingCaseId);
            if (!listingCaseResponse.Succeed)
                return ApiResponse<object?>.Fail(listingCaseResponse.Message ?? "listing case Unknown error.", listingCaseResponse.ErrorCode);

            ListingCase listingCase = listingCaseResponse.Data!;
            var agentListingCases = new List<AgentListingCase>();

            foreach (string agentId in agentIds)
            {
                ApiResponse<object?> validationResponse = await _validator.ValidateAgentAndListingCaseAsync(agentId, listingCaseId);
                if (!validationResponse.Succeed)
                    return ApiResponse<object?>.Fail(validationResponse.Message ?? "Agent and listing case validation failed.", validationResponse.ErrorCode);
                var agentResult = await _validator.ValidateAgentAsync(agentId);
                if (!agentResult.Succeed)
                    return ApiResponse<object?>.Fail(agentResult.Message ?? "listing case Unknown error.", agentResult.ErrorCode);

                var agent = agentResult.Data!;
                AgentListingCase agentListingCase = new AgentListingCase
                {
                    AgentId = agent.Id,
                    ListingCaseId = listingCase.Id,
                    Agent = agent,
                    ListingCase = listingCase
                };
                agent.AgentListingCases.Add(agentListingCase);
                listingCase.AgentListingCases.Add(agentListingCase);
                await _repository.AddAgentListingCaseAsync(agentListingCase);
                await _generalRepository.SaveChangesAsync();
                agentListingCases.Add(agentListingCase);
                
            }
            await transaction.CommitAsync();
            return ApiResponse<object?>.Success(null, "Agents added to listing case successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ApiResponse<object?>.Fail($"Error adding agents to listing case: {ex.Message}", "500");
        }
    }

    public async Task<ApiResponse<ICollection<ListingCase>>> GetAllListingCasesByAgentAsync(string currentUserId)
    {
        var agentResult = await _validator.ValidateAgentAsync(currentUserId);
        if (!agentResult.Succeed)
            return ApiResponse<ICollection<ListingCase>>.Fail(agentResult.Message ?? "Agent validation failed.", agentResult.ErrorCode);
        Agent agent = agentResult.Data!;
        ICollection<ListingCase> listingCases =  _repository.GetAllListingCasesByAgentAsync(agent);
        return ApiResponse<ICollection<ListingCase>>.Success(listingCases, "Listing cases retrieved successfully.");
    }

    public async Task<ApiResponse<ICollection<ListingCase>>> GetAllListingCasesByCreatorAsync(string currentUserId)
    {
        User? currentUser = await _userManager.FindByIdAsync(currentUserId);
        if (currentUser == null)
        {
            return ApiResponse<ICollection<ListingCase>>.Fail("Current user not found.", "404");
        }
        if (currentUser.IsDeleted)
        {
            return ApiResponse<ICollection<ListingCase>>.Fail("Current user is deleted.", "403");
        }
        ICollection<ListingCase> listingCases = await _repository.GetAllListingCasesByUserAsync(currentUser);
        return ApiResponse<ICollection<ListingCase>>.Success(listingCases, "Listing cases retrieved successfully.");
    }

    public async Task<ApiResponse<ListingCase>> DeleteListingCaseAsync(string listingCaseId)
    {
        try
        {
            ApiResponse<ListingCase?> listingCaseResponse = await _validator.ValidateListingCaseAsync(listingCaseId);
            if (!listingCaseResponse.Succeed)
                return ApiResponse<ListingCase>.Fail(listingCaseResponse.Message ?? "listing case Unknown error.", listingCaseResponse.ErrorCode);

            ListingCase listingCase = listingCaseResponse.Data!;
             _repository.DeleteListingCase(listingCase);
            await _generalRepository.SaveChangesAsync();
            return ApiResponse<ListingCase>.Success(listingCase, "Listing case deleted successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse<ListingCase>.Fail($"Error deleting listing case: {ex.Message}", "500");
        }
        
    }

}