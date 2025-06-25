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

    public async Task<ApiResponse<object?>> CreateListingCaseAsync(ListingCaseDto listingCaseDto, User user)
    {
        if (listingCaseDto == null)
        {
            throw new ArgumentNullException(nameof(listingCaseDto), "Listing case cannot be null.");
        }
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user), "User cannot be null.");
        }
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
            
        } catch (Exception ex)
        {
            return ApiResponse<object?>.Fail($"Error adding agents to listing case: {ex.Message}", "500");
        }
   
    }

    public async Task<ICollection<ListingCase>> GetAllListingCasesAsync()
    {
        return await _dbContext.ListingCases.ToListAsync();
    }
    
}