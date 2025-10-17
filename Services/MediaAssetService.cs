using Microsoft.AspNetCore.Identity;
using RecamSystemApi.Data;
using RecamSystemApi.Models;
using RecamSystemApi.Utility;

public class MediaAssetService : IMediaAssetService
{
    private readonly IAzureBlobStorageService _azureBlobStorageService;
    private readonly IMediaAssetRepository _mediaAssetRepository;

    private readonly IGeneralRepository _generalRepository;
    private readonly IAgentListingCaseValidator _agentListingCaseValidator;
    private readonly UserManager<User> _userManager;

    public MediaAssetService(IGeneralRepository generalRepository, IAzureBlobStorageService azureBlobStorageService, IMediaAssetRepository mediaAssetRepository, IAgentListingCaseValidator agentListingCaseValidator, UserManager<User> userManager)
    {
        _generalRepository = generalRepository;
        _azureBlobStorageService = azureBlobStorageService;
        _mediaAssetRepository= mediaAssetRepository;
        _agentListingCaseValidator = agentListingCaseValidator;
        _userManager = userManager;
    }

    public async Task<ApiResponse<ICollection<MediaAssetDto?>>> UploadMediaAssetsBulkAsync(ICollection<IFormFile> files, string userId, string listingCaseId, MediaType mediaType)
    {
        using var transaction = await _generalRepository.BeginTransactionAsync();
        try
        {
            ICollection<MediaAssetDto?> uploadedMediaAssets = new List<MediaAssetDto?>();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ApiResponse<ICollection<MediaAssetDto?>>.Fail($"User with ID {userId} not found.", "404");
            if (user.IsDeleted)
                return ApiResponse<ICollection<MediaAssetDto?>>.Fail($"User with ID {userId} is deleted.", "403");
            if (files == null || files.Count == 0)
                return ApiResponse<ICollection<MediaAssetDto?>>.Fail("File cannot be null or empty.", "400");

            ApiResponse<ListingCase?> listingCaseResponse = await _agentListingCaseValidator.ValidateListingCaseAsync(listingCaseId);
            if (!listingCaseResponse.Succeed || listingCaseResponse.Data == null)
                return ApiResponse<ICollection<MediaAssetDto?>>.Fail(listingCaseResponse.ErrorMessage ?? "Unknown error occurred.", listingCaseResponse.ErrorCode);
            ListingCase listingCase = listingCaseResponse.Data;
            IEnumerable<(IFormFile file, string mediaType)> fileList = files.Select(f => (f, mediaType.ToString()));

            IDictionary<string, string> blobUrls = await _azureBlobStorageService.UploadFileBulkAsync(fileList);
            foreach (var (file, url) in blobUrls)
            {
                //GUID_filename_mediatype
                var segments = file.Split('_');
                if (segments.Length < 3)
                {
                    return ApiResponse<ICollection<MediaAssetDto?>>.Fail("Invalid file name format.", "400");
                }

                string fileName = segments[1];
                string type = segments[2];

                MediaAssetDto mediaAssetDto = new MediaAssetDto
                {
                    FileName = fileName,
                    FilePath = url,
                    MediaType = Enum.Parse<MediaType>(type),
                    UserId = user.Id,
                    ListingCaseId = listingCase.Id,
                };

                MediaAsset mediaAsset = _generalRepository.MapDto<MediaAssetDto, MediaAsset>(mediaAssetDto);
                mediaAsset.ListingCase = listingCase;
                mediaAsset.User = user;
                user.MediaAssets.Add(mediaAsset);
                listingCase.MediaAssets.Add(mediaAsset);
                await _mediaAssetRepository.AddMediaAssetAsync(mediaAsset);
                await _generalRepository.SaveChangesAsync();
                await transaction.CommitAsync();
                uploadedMediaAssets.Add(mediaAssetDto);
            }
            return ApiResponse<ICollection<MediaAssetDto?>>.Success(uploadedMediaAssets, "Uploaded");

        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ApiResponse<ICollection<MediaAssetDto?>>.Fail($"Error uploading media assets: {ex.Message}", "500");
        }
 

    }

    // Other methods omitted for brevity...
}