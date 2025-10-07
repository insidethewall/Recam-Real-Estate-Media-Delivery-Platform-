using RecamSystemApi.Models;

public interface IMediaAssetRepository
{ 
    Task<MediaAssetDto> UploadMediaAssetAsync(IFormFile file, string userId, string listingCaseId);
    Task<List<MediaAssetDto>> UploadMediaAssetsBulkAsync(IEnumerable<(IFormFile file, string pageName)> files, string userId, string listingCaseId);
    Task<MediaAssetDto> GetMediaAssetByUserIdAsync(string userId);
    Task<List<MediaAssetDto>> GetMediaAssetsByListingCaseIdAsync(string listingCaseId);
    Task<bool> DeleteMediaAssetAsync(int id);
}