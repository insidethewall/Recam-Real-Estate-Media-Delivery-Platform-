using Microsoft.AspNetCore.Identity;
using RecamSystemApi.Exception;
using RecamSystemApi.Models;

public class MediaAssetService : IMediaAssetService
{
    private readonly IAzureBlobStorageService _azureBlobStorageService;
    private readonly IMediaAssetRepository _mediaAssetRepository;

    private readonly IGeneralRepository _generalRepository;
    private readonly IAgentListingCaseValidator _agentListingCaseValidator;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<MediaAssetService> _logger;

    public MediaAssetService(IGeneralRepository generalRepository, IAzureBlobStorageService azureBlobStorageService, IMediaAssetRepository mediaAssetRepository, IAgentListingCaseValidator agentListingCaseValidator, UserManager<User> userManager, ILogger<MediaAssetService> logger)
    {
        _generalRepository = generalRepository;
        _azureBlobStorageService = azureBlobStorageService;
        _mediaAssetRepository = mediaAssetRepository;
        _agentListingCaseValidator = agentListingCaseValidator;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ICollection<MediaAssetDto>> UploadMediaAssetsBulkAsync(ICollection<IFormFile> files, string userId, string listingCaseId, MediaType mediaType)
    {
        using var transaction = await _generalRepository.BeginTransactionAsync();
        try
        {
            ICollection<MediaAssetDto> uploadedMediaAssets = new List<MediaAssetDto>();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new UserNotFoundException($"User with ID {userId} not found.");
            if (user.IsDeleted)
                throw new UserNotFoundException($"User account with ID {userId} is deleted.");
            if (files == null || files.Count == 0)
                throw new ArgumentException("No files provided for upload.");

            ListingCase listingCase = await _agentListingCaseValidator.ValidateListingCaseAsync(listingCaseId);
            IEnumerable<(IFormFile file, string mediaType)> fileList = files.Select(f => (f, mediaType.ToString()));

            IDictionary<string, string> blobUrls = await _azureBlobStorageService.UploadFileBulkAsync(fileList);
            foreach (var (file, url) in blobUrls)
            {
                //GUID_filename_mediatype
                var segments = file.Split('_');
                if (segments.Length < 3)
                {
                    throw new FormatException($"Uploaded file name '{file}' is not in the expected format.");
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
                await _mediaAssetRepository.AddMediaAssetAsync(mediaAsset);

                uploadedMediaAssets.Add(mediaAssetDto);
            }
            await _generalRepository.SaveChangesAsync();
            await transaction.CommitAsync();
            return uploadedMediaAssets;

        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"Error uploading media assets: {ex.Message}");
        }


    }
    public async Task<ICollection<MediaAsset>> GetMediaAssetsByListingCaseAsync(string listingCaseId)
    {
        ListingCase listingCase = await _agentListingCaseValidator.ValidateListingCaseAsync(listingCaseId);
        ICollection<MediaAsset> mediaAssets = await _mediaAssetRepository.GetMediaAssetsByListingCaseAsync(listingCase.Id);
        _logger.LogInformation($"Retrieved {mediaAssets.Count} media assets for ListingCase ID {listingCaseId}.");
        return mediaAssets;
    }

    public async Task<MediaAsset> DeleteMediaAssetAsync(string mediaAssetId)
    {
        using var transaction = await _generalRepository.BeginTransactionAsync();
        try
        {
            MediaAsset mediaAsset = await _mediaAssetRepository.GetMediaAssetByIdAsync(mediaAssetId);
            if (mediaAsset == null)
                throw new NotFoundException($"Media asset with ID {mediaAssetId} not found.");


            _mediaAssetRepository.DeleteMediaAssetAsync(mediaAsset);
            bool isDeleted = await _azureBlobStorageService.DeleteFileAsync(mediaAsset.FilePath);
            if (!isDeleted)
            {
                throw new Exception($"Failed to delete the file from Azure Blob Storage at {mediaAsset.FilePath}.");
            }
            await _generalRepository.SaveChangesAsync();
            await transaction.CommitAsync();
            return mediaAsset;

        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new Exception($"Error deleting media asset: {ex.Message}");
        }

    }

    public async Task<ICollection<MediaAsset>> SelectMediaAssetsByListingCase(string listingCaseId, List<string> mediaAssetIds)
    {
        await using var transaction = await _generalRepository.BeginTransactionAsync();
        try
        {
            ListingCase listingCase = await _agentListingCaseValidator.ValidateListingCaseAsync(listingCaseId);
            ICollection<MediaAsset> mediaAssets = await _mediaAssetRepository.GetMediaAssetsByListingCaseAsync(listingCaseId);
            ICollection<MediaAsset> selectedMediaAssets = new List<MediaAsset>();
            foreach (var ma in mediaAssets)
            {
                ma.IsDisplaySelected = mediaAssetIds.Contains(ma.Id);
                selectedMediaAssets.Add(ma);
            }
            await _generalRepository.SaveChangesAsync();
            await transaction.CommitAsync();
            return selectedMediaAssets;

        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw new Exception("Error selecting media assets for listing case.");
        }
    }
    
    public async Task<MediaAsset> SelectHeroMediaAsset(string listingCaseId, string mediaAssetId)
    {
        await using var transaction = await _generalRepository.BeginTransactionAsync();
        try
        {
            ListingCase listingCase = await _agentListingCaseValidator.ValidateListingCaseAsync(listingCaseId);
            ICollection<MediaAsset> mediaAssets = await _mediaAssetRepository.GetMediaAssetsByListingCaseAsync(listingCaseId);
            MediaAsset? heroMediaAsset = null;
            foreach (var ma in mediaAssets)
            {
                if (ma.Id == mediaAssetId)
                {
                    ma.IsHeroMedia = true;
                    heroMediaAsset = ma;
                }
                else
                {
                    ma.IsHeroMedia = false;
                }
            }
            if (heroMediaAsset == null)
                throw new NotFoundException($"Media asset with ID {mediaAssetId} not found in listing case {listingCaseId}.");

            await _generalRepository.SaveChangesAsync();
            await transaction.CommitAsync();
            return heroMediaAsset;

        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw new Exception("Error selecting hero media asset for listing case.");
        }
    }

    // Other methods omitted for brevity...
}