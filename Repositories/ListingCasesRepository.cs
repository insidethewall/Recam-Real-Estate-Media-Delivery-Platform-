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

    public async Task AddAgentListingCaseAsync(AgentListingCase agentListingCase)
    {
        await _dbContext.AgentListingCases.AddAsync(agentListingCase);
 
    }

    public async Task AddListingCaseAsync(ListingCase listingCase)
    {
        await _dbContext.ListingCases.AddAsync(listingCase);
   
    }

    public async Task<ICollection<ListingCase>> GetAllListingCasesByUserAsync(User currentUser)
    {
        ICollection<ListingCase> listingCases = await _dbContext.ListingCases
            .Where(lc => lc.UserId == currentUser.Id && !lc.IsDeleted)
            .ToListAsync();

        return listingCases;
    }

    public async Task<ListingCase> GetListingCaseByIdAsync(string listingCaseId)
    {
        ListingCase listingCase = await _dbContext.ListingCases.FirstAsync(lc => lc.Id == listingCaseId && !lc.IsDeleted);

        return listingCase;
    }

    public ICollection<ListingCase> GetAllListingCasesByAgentAsync(Agent agent)
    {
        ICollection<ListingCase> listingCases = agent.AgentListingCases
            .Where(alc => !alc.ListingCase.IsDeleted)
            .Select(alc => alc.ListingCase)
            .ToList();

        return listingCases;
    }

    public async Task<ICollection<ListingCase>> GetAllListingCasesAsync()
    {
        return await _dbContext.ListingCases.Include(lc=>lc.User).ToListAsync();
    }
    
    public void DeleteListingCase(ListingCase listingCase)
    {
      
        listingCase.IsDeleted = true;
        _dbContext.ListingCases.Update(listingCase);
  
    }
    
}