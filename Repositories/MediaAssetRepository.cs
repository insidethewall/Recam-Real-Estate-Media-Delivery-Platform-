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

    public async Task<ApiResponse<MediaAssetDto>> UploadMediaAssetAsync(string file, string url, User user, ListingCase listingCase)
    {
        try
        {
            //GUID_filename_mediatype
            var segments = file.Split('_');
            if (segments.Length < 3)
            {
                return ApiResponse<MediaAssetDto>.Fail("Invalid file name format.", "400");
            }

            string fileName = segments[1];
            string mediaType = segments[2];
     
            MediaAssetDto mediaAssetDto = new MediaAssetDto
            {
                FileName = fileName,
                FilePath = url,
                MediaType = Enum.Parse<MediaType>(mediaType),
                UserId = user.Id,
                ListingCaseId = listingCase.Id,
            };
            MediaAsset mediaAsset = _mapper.Map<MediaAsset>(mediaAssetDto);
            mediaAsset.ListingCase = listingCase;
            mediaAsset.User = user;
            user.MediaAssets.Add(mediaAsset);
            listingCase.MediaAssets.Add(mediaAsset);
            _dbContext.MediaAssets.Add(mediaAsset);
            await _dbContext.SaveChangesAsync();
            return ApiResponse<MediaAssetDto>.Success(mediaAssetDto, "Uploaded");
        }
        catch (Exception ex)
        {
            return ApiResponse<MediaAssetDto>.Fail($"Error uploading media asset: {ex.Message}", "500");
        }
    }
}
