using Microsoft.AspNetCore.Authorization;
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


    [Authorize(Roles = "Admin, Photographer")]
    [HttpPost("upload")]
    public async Task<IActionResult> UploadMediaAssetAsync
    ([FromForm] ICollection<IFormFile> files, [FromQuery] string userId, [FromQuery] string listingCaseId, [FromQuery] MediaType mediaType)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest("File cannot be null or empty.");
        }

        var response = await _mediaAssetService.UploadMediaAssetsBulkAsync (files, userId, listingCaseId, mediaType);
        if (response.Succeed)
        {
            return Ok(response.Data);
        }
        return BadRequest(response.ErrorMessage);
    }

    // Other methods for bulk upload, retrieval, and deletion can be added here...

}