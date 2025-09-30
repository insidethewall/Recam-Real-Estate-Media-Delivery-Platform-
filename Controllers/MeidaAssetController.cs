using Microsoft.AspNetCore.Mvc;

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

    [HttpPost("upload")]
    public async Task<IActionResult> UploadMediaAsset([FromForm] ICollection<IFormFile> files, [FromQuery] string userId, [FromQuery] string listingCaseId)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest("File cannot be null or empty.");
        }

        var response = await _mediaAssetService.UploadMediaAssetAsync(file, userId, listingCaseId);
        if (response.Succeed)
        {
            return Ok(response.Data);
        }
        return BadRequest(response.ErrorMessage);
    }

    // Other methods for bulk upload, retrieval, and deletion can be added here...

}