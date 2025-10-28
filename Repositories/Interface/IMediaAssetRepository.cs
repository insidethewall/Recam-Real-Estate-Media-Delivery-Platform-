using RecamSystemApi.Models;
using RecamSystemApi.Utility;

public interface IMediaAssetRepository
{
    Task AddMediaAssetAsync(MediaAsset mediaAsset);
    Task<ICollection<MediaAsset>> GetMediaAssetsByListingCaseAsync(string listingCaseId);
    Task<MediaAsset> GetMediaAssetByIdAsync(string mediaAssetId);
    void DeleteMediaAssetAsync(MediaAsset mediaAsset);
   
    // Task<MediaAssetDto> UploadMediaAssetAsync(IFormFile file, string userId, string listingCaseId);
    // Task<MediaAssetDto> GetMediaAssetByUserIdAsync(string userId);
    // Task<List<MediaAssetDto>> GetMediaAssetsByListingCaseIdAsync(string listingCaseId);
    // Task<bool> DeleteMediaAssetAsync(int id);
}