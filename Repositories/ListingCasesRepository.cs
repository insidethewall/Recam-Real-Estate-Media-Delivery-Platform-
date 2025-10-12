using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RecamSystemApi.Data;
using RecamSystemApi.DTOs;
using RecamSystemApi.Models;
using RecamSystemApi.Utility;

public class ListingCasesRepository : IListingCasesRepository
{
    private readonly ReacmDbContext _dbContext;
    private readonly IMapper _mapper;

    public ListingCasesRepository(ReacmDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<ApiResponse<UpdateListingCaseDto?>> UpdateListingCaseAsync(UpdateListingCaseDto listingCaseDto, string listingCaseId)
    {
        try
        {
            ListingCase? existingListingCase = await _dbContext.ListingCases
                .FirstOrDefaultAsync(lc => lc.Id == listingCaseId && !lc.IsDeleted);
            if (existingListingCase == null)
            {
                return ApiResponse<UpdateListingCaseDto?>.Fail("Listing case not found.", "404");
            }

            _mapper.Map(listingCaseDto, existingListingCase);
            await _dbContext.SaveChangesAsync();

            UpdateListingCaseDto updatedDto = _mapper.Map<UpdateListingCaseDto>(existingListingCase);
            return ApiResponse<UpdateListingCaseDto?>.Success(updatedDto, "Listing case updated successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse<UpdateListingCaseDto?>.Fail($"Error updating listing case: {ex.Message}", "500");
        }

    }

    public async Task<ApiResponse<ListingCaseStatusDto?>> ChangeListingCaseStatusAsync(ListcaseStatus newStatus, ListingCase listingCase)
    {
        try
        {

            listingCase.ListcaseStatus = newStatus;
            await _dbContext.SaveChangesAsync();

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
 

    public async Task<ApiResponse<object?>> CreateListingCaseAsync(ListingCaseDto listingCaseDto, User user)
    {
        try
        {
            ListingCase listingCase = _mapper.Map<ListingCase>(listingCaseDto);
            listingCase.UserId = user.Id;
            listingCase.User = user;
            _dbContext.ListingCases.Add(listingCase);
            await _dbContext.SaveChangesAsync();
            return ApiResponse<object?>.Success(listingCaseDto, "Listing case created successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse<object?>.Fail($"Error creating listing case: {ex.Message}", "500");
        }

    }

    public async Task<ApiResponse<object?>> CreateAgentListingCaseAsync(Agent agent, ListingCase listingCase)
    {
        try
        {
            AgentListingCase agentListingCase = new AgentListingCase
            {
                AgentId = agent.Id,
                ListingCaseId = listingCase.Id,
                Agent = agent,
                ListingCase = listingCase
            };
            agent.AgentListingCases.Add(agentListingCase);
            listingCase.AgentListingCases.Add(agentListingCase);
            _dbContext.Agents.Update(agent);
            _dbContext.ListingCases.Update(listingCase);
            _dbContext.AgentListingCases.Add(agentListingCase);
            await _dbContext.SaveChangesAsync();
            return ApiResponse<object?>.Success(null, "Agents added to listing case successfully.");

        }
        catch (Exception ex)
        {
            return ApiResponse<object?>.Fail($"Error adding agents to listing case: {ex.Message}", "500");
        }

    }

    public async Task<ApiResponse<ICollection<ListingCase>>> GetAllListingCasesByUserAsync(User currentUser)
    {
        ICollection<ListingCase> listingCases = await _dbContext.ListingCases
            .Where(lc => lc.UserId == currentUser.Id && !lc.IsDeleted)
            .ToListAsync();

        return ApiResponse<ICollection<ListingCase>>.Success(listingCases, "Listing cases retrieved successfully.");
    }

    public ApiResponse<ICollection<ListingCase>> GetAllListingCasesByAgentAsync(Agent agent)
    {
        ICollection<ListingCase> listingCases = agent.AgentListingCases
            .Where(alc => !alc.ListingCase.IsDeleted)
            .Select(alc => alc.ListingCase)
            .ToList();

        return ApiResponse<ICollection<ListingCase>>.Success(listingCases, "Listing cases retrieved successfully.");
    }

    public async Task<ICollection<ListingCase>> GetAllListingCasesAsync()
    {
        return await _dbContext.ListingCases.ToListAsync();
    }
    
    public async Task<ApiResponse<ListingCase>> DeleteListingCaseAsync(ListingCase listingCase)
    {
      
        listingCase.IsDeleted = true;
        _dbContext.ListingCases.Update(listingCase);
        await _dbContext.SaveChangesAsync();
        return ApiResponse<ListingCase>.Success(listingCase, "Listing case deleted successfully.");
  
    }
    
}