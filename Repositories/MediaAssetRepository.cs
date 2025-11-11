using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RecamSystemApi.Data;
using RecamSystemApi.Exception;
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

    public async Task<MediaAsset> GetMediaAssetByIdAsync(string mediaAssetId)
    {
        MediaAsset? mediaAsset = await _dbContext.MediaAssets.Include(ma=>ma.ListingCase).ThenInclude(l=>l.User)
            .FirstOrDefaultAsync(ma => ma.Id == mediaAssetId);
        if (mediaAsset == null)
            throw new NotFoundException($"Media asset with ID {mediaAssetId} not found.");
        return mediaAsset;
    }

    public void DeleteMediaAssetAsync(MediaAsset mediaAsset)
    {
        _dbContext.MediaAssets.Remove(mediaAsset);
    }


}
