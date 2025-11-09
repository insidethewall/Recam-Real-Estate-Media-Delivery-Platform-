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
     private readonly IListingCasesLogRepository _listingCasesLogRepository;
    private readonly ILogger<MediaAssetService> _logger;

    public MediaAssetService(IGeneralRepository generalRepository, IAzureBlobStorageService azureBlobStorageService, IMediaAssetRepository mediaAssetRepository, IAgentListingCaseValidator agentListingCaseValidator, UserManager<User> userManager, ILogger<MediaAssetService> logger, IListingCasesLogRepository listingCasesLogRepository)
    {
        _generalRepository = generalRepository;
        _azureBlobStorageService = azureBlobStorageService;
        _mediaAssetRepository = mediaAssetRepository;
        _agentListingCaseValidator = agentListingCaseValidator;
        _userManager = userManager;
        _logger = logger;
        _listingCasesLogRepository = listingCasesLogRepository;
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
            List<FieldChange> fieldChanges = new List<FieldChange>();

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
                FieldChange fieldChange = new FieldChange("MediaAsset[+]", "null", mediaAsset.Id);
                fieldChanges.Add(fieldChange);
            }
            await _generalRepository.SaveChangesAsync();

            ListingCaseLog listingCaseLog = await _listingCasesLogRepository.CreateListingCaseLog(listingCase, ChangeType.Updated, listingCase.User ?? throw new Exception("creator of listingcase log cannot be null"), listingCase.User,"",fieldChanges);
            await _listingCasesLogRepository.AddLog(listingCaseLog);
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
 
        var asset = await _mediaAssetRepository.GetMediaAssetByIdAsync(mediaAssetId);
    if (asset is null)
        throw new NotFoundException($"Media asset with ID {mediaAssetId} not found.");
    if (string.IsNullOrWhiteSpace(asset.FilePath))
        throw new InvalidOperationException($"Media asset {mediaAssetId} has no FilePath.");

    // 1) DB FIRST: mark as soft-deleted and pending blob delete, then commit
    await using (var tx = await _generalRepository.BeginTransactionAsync())
    {
        asset.IsDeleted = true;            // hide from UI/business logic
        asset.BlobDeleted = false;         // not yet deleted in storage
        asset.BlobDeletePending = true;    // <-- add this bool column
        var fieldChanges = new List<FieldChange>
        {
            new FieldChange(
                "MediaAsset[-]", 
                OldValue: asset.Id,
                NewValue: "null"
            )
        };

            // Load listing case for the log
        var listingCase = asset.ListingCase;

        var log = await _listingCasesLogRepository.CreateListingCaseLog(
            listingCase,
            ChangeType.Updated, 
            listingCase.User ?? throw new Exception("creator of listingcase log cannot be null"),
            listingCase.User,
            "Blob deleted; media asset fully removed.",
            fieldChanges
        );

        await _listingCasesLogRepository.AddLog(log);
        await _generalRepository.SaveChangesAsync();
        await tx.CommitAsync();
    }

    // 2) BLOB SECOND: try to delete the blob outside the DB transaction
    try
    {
        var deleted = await _azureBlobStorageService.DeleteFileAsync(asset.FilePath);
        if (!deleted)
            throw new InvalidOperationException($"Blob delete returned false for path {asset.FilePath}.");

        // 3) Mark success in DB
        asset.BlobDeletePending = false;
        asset.BlobDeleted = true;
        _mediaAssetRepository.DeleteMediaAssetAsync(asset);
        await _generalRepository.SaveChangesAsync();

        return asset;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Blob deletion failed for asset {AssetId} (path: {Path})", asset.Id, asset.FilePath);
        return asset;
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