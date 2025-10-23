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

        ICollection<MediaAssetDto> response = await _mediaAssetService.UploadMediaAssetsBulkAsync (files, userId, listingCaseId, mediaType);
        return Ok(ApiResponse<ICollection<MediaAssetDto>>.Success(response, "Files uploaded successfully."))
            ;
    }

    // Other methods for bulk upload, retrieval, and deletion can be added here...

}