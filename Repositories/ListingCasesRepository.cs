using Microsoft.EntityFrameworkCore;
using RecamSystemApi.Data;
using RecamSystemApi.Models;

public class ListingCasesRepository : IListingCasesRepository
{
    private readonly ReacmDbContext _dbContext;

    public ListingCasesRepository(ReacmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ICollection<ListingCase>> GetAllListingCasesAsync()
    {
        return await _dbContext.ListingCases.ToListAsync();
    }
    
}