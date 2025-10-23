using RecamSystemApi.Models;
using RecamSystemApi.Utility;

public interface IMediaAssetService
{
    // Task<ApiResponse<MediaAssetDto>> UploadMediaAssetAsync(IFormFile file, string userId, string listingCaseId);
    Task<ICollection<MediaAssetDto>> UploadMediaAssetsBulkAsync(ICollection<IFormFile> files, string userId, string listingCaseId, MediaType mediaType);
    // Task<MediaAssetDto> GetMediaAssetByUserIdAsync(string userId);
    // Task<List<MediaAssetDto>> GetMediaAssetsByListingCaseIdAsync(string listingCaseId);
    // Task<bool> DeleteMediaAssetAsync(int id);
}