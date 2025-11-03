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
        ListingCase listingCase = await _dbContext.ListingCases
            .Include(lc => lc.MediaAssets)
            .Include(lc => lc.AgentListingCases)
            .Include(lc => lc.CaseContacts)
            .Include(lc=>lc.User)
            .FirstAsync(lc => lc.Id == listingCaseId && !lc.IsDeleted);

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

    public async Task<ICollection<ListingCaseWithNavDto>> GetAllListingCasesAsync()
    {
        var listings = await _dbContext.ListingCases
            .Include(lc => lc.User)
            .Include(lc => lc.MediaAssets)
            .Include(lc => lc.AgentListingCases)
            .Include(lc => lc.CaseContacts)
            .Where(lc => !lc.IsDeleted)
            .ToListAsync();

        var dtoList = _mapper.Map<List<ListingCaseWithNavDto>>(listings);
        return dtoList;
    }
       public async Task<ICollection<ListingCaseWithNavDto>> GetAllDeletedListingCasesAsync()
    {
        var listings = await _dbContext.ListingCases
            .Include(lc => lc.User)
            .Include(lc => lc.MediaAssets)
            .Include(lc => lc.AgentListingCases)
            .Include(lc => lc.CaseContacts)
            .Where(lc => lc.IsDeleted)
            .ToListAsync();

        var dtoList = _mapper.Map<List<ListingCaseWithNavDto>>(listings);
        return dtoList;
    }


    public void SoftDeleteListingCase(ListingCase listingCase)
    {

        listingCase.IsDeleted = true;
        _dbContext.ListingCases.Update(listingCase);

    }


  // TODO: DO I need to unbind the relationships of listing case when soft delete?
    public void SoftDeleteMediaAssetsByListingCase(ListingCase listingCase)
    {
        ICollection<MediaAsset> mediaAssets = _dbContext.MediaAssets.Where(ma => ma.ListingCaseId == listingCase.Id).ToList();
        foreach (var mediaAsset in mediaAssets)
        {
            mediaAsset.IsDeleted = true;
        }
        _dbContext.MediaAssets.UpdateRange(mediaAssets);

    }

    public void DeleteAgentListingCase(ListingCase listingCase)
    {
        var agentListingCases = _dbContext.AgentListingCases.Where(alc => alc.ListingCaseId == listingCase.Id);
        _dbContext.AgentListingCases.RemoveRange(agentListingCases);

    }

    public void RemoveListingCaseFromUser(ListingCase listingCase)
    {
        User? user = _dbContext.Users
         .Include(u => u.ListingCases)
         .FirstOrDefault(u => u.Id == listingCase.UserId);

        if (user != null)
        {
            user.ListingCases.Remove(listingCase); // Removes from navigation property
        }
        listingCase.UserId = null; // Clears the foreign key
        listingCase.User = null;   // Clears the navigation property

    }
    
}