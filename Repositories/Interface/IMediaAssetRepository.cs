using RecamSystemApi.Models;
using RecamSystemApi.Utility;

public interface IMediaAssetRepository
{ 
    // Task<MediaAssetDto> UploadMediaAssetAsync(IFormFile file, string userId, string listingCaseId);
    Task<ApiResponse<MediaAssetDto>> UploadMediaAssetAsync(string fileName, string url , User user, ListingCase listingCase);
    // Task<MediaAssetDto> GetMediaAssetByUserIdAsync(string userId);
    // Task<List<MediaAssetDto>> GetMediaAssetsByListingCaseIdAsync(string listingCaseId);
    // Task<bool> DeleteMediaAssetAsync(int id);
}