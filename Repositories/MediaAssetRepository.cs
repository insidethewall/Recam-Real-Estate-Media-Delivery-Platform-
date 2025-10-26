using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RecamSystemApi.Data;
using RecamSystemApi.Models;
using RecamSystemApi.Utility;

public class MediaAssetRepository : IMediaAssetRepository
{
    private readonly ReacmDbContext _dbContext;
    private readonly IMapper _mapper;

    public MediaAssetRepository(ReacmDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task AddMediaAssetAsync(MediaAsset mediaAsset)
    {
        await _dbContext.MediaAssets.AddAsync(mediaAsset);

    }
    public async Task<ICollection<MediaAsset>> GetMediaAssetsByListingCaseAsync(string listingCaseId)
    {
        return await _dbContext.MediaAssets
            .Where(ma => ma.ListingCaseId == listingCaseId && !ma.IsDeleted)
            .ToListAsync();
    }
}
