using Microsoft.AspNetCore.Identity;
using RecamSystemApi.Models;
using RecamSystemApi.Utility;

public class MediaAssetService : IMediaAssetService
{
    private readonly IAzureBlobStorageService _azureBlobStorageService;
      private readonly UserManager<User> _userManager;

    public MediaAssetService(IAzureBlobStorageService azureBlobStorageService, UserManager<User> userManager)
    {
        _azureBlobStorageService = azureBlobStorageService;
        _userManager = userManager;
    }

    public async Task<ApiResponse<MediaAssetDto?>> UploadMediaAssetAsync(IFormFile file, string userId, string listingCaseId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return ApiResponse<MediaAssetDto?>.Fail($"User with ID {userId} not found.", "404");

        if (user.IsDeleted)
            return ApiResponse<MediaAssetDto?>.Fail($"User with ID {userId} is deleted.", "403");
        if (file == null || file.Length == 0)
            return ApiResponse<MediaAssetDto?>.Fail("File cannot be null or empty.", "400");

        var blobUrl = await _azureBlobStorageService.UploadFileAsync(file, "media-assets");

    }

    // Other methods omitted for brevity...
}