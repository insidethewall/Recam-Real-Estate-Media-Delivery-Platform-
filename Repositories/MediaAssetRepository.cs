using AutoMapper;
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

    public Task AddMediaAssetAsync(MediaAsset mediaAsset)
    {
        _dbContext.MediaAssets.Add(mediaAsset);
        return Task.CompletedTask;
    }
}
