using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecamSystemApi.Models;
using RecamSystemApi.Utility;

namespace RecamSystemApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaAssetController : ControllerBase
{
    private readonly IMediaAssetService _mediaAssetService;

    public MediaAssetController(IMediaAssetService mediaAssetService)
    {
        _mediaAssetService = mediaAssetService;
    }


    [Authorize(Roles = "Admin, Photographer")]
    [HttpPost("upload")]
    public async Task<IActionResult> UploadMediaAssetAsync([FromForm] ICollection<IFormFile> files, [FromQuery] string listingCaseId, [FromQuery] MediaType mediaType)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest(ApiResponse<string>.Fail("No files provided for upload.", "400"));
        }

        string? userId = User.FindFirst("UserId")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<string>.Fail("User ID not found in token.", "401"));
        }

        ICollection<MediaAssetDto> response = await _mediaAssetService.UploadMediaAssetsBulkAsync(files, userId, listingCaseId, mediaType);
        return Ok(ApiResponse<ICollection<MediaAssetDto>>.Success(response, "Files uploaded successfully."));
    }
    [HttpGet("listings/{listingId}/media")]
    public async Task<IActionResult> GetMediaAssetsByListingCaseAsync([FromRoute] string listingId)
    {
        ICollection<MediaAsset> response = await _mediaAssetService.GetMediaAssetsByListingCaseAsync(listingId);
        return Ok(ApiResponse<ICollection<MediaAsset>>.Success(response, "Media assets retrieved successfully."));

    }

    [Authorize(Roles = "Admin, Photographer")]
    [HttpDelete("delete/{mediaAssetId}")]
    public async Task<IActionResult> DeleteMediaAssetAsync([FromRoute] string mediaAssetId)
    {

        MediaAsset deletedMediaAsset = await _mediaAssetService.DeleteMediaAssetAsync(mediaAssetId);
        return Ok(ApiResponse<MediaAsset>.Success(deletedMediaAsset, "Media asset deleted successfully."));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("listings/{listingId}/selectedMedia")]

    public async Task<IActionResult> SelectMediaAssetsForListingCase([FromRoute] string listingId, [FromBody] List<string> mediaAssetIds)
    {

        ICollection<MediaAsset> mediaAssets = await _mediaAssetService.SelectMediaAssetsByListingCase(listingId, mediaAssetIds);
        return Ok(ApiResponse<ICollection<MediaAsset>>.Success(mediaAssets, "Media assets selected successfully."));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("listings/{listingId}/selectedHero")]
    public async Task<IActionResult> SelectHeroMediaAssetForListingCase([FromRoute] string listingId, [FromBody] string mediaAssetId)
    {

        MediaAsset mediaAsset = await _mediaAssetService.SelectHeroMediaAsset(listingId, mediaAssetId);
        return Ok(ApiResponse<MediaAsset>.Success(mediaAsset, "Hero media asset selected successfully."));
    }
    
    



}