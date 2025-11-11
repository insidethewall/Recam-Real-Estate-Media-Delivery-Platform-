using Microsoft.AspNetCore.Mvc;
using RecamSystemApi.Utility;

[Route("api/[controller]")]
[ApiController]
public class ListingCaseLogController : ControllerBase
{

    private readonly IListingCasesLogRepository _listingCasesLogRepository;

    public ListingCaseLogController( IListingCasesLogRepository listingCasesLogRepository)
    {
        _listingCasesLogRepository = listingCasesLogRepository;
    }

    [HttpGet("allLogs")]
    public async Task<IActionResult> GetAllLogs()
    {
        try
        {
            ICollection<ListingCaseLog> logs = await _listingCasesLogRepository.GetAllListingCasesLog();


            return Ok(ApiResponse<ICollection<ListingCaseLog>>.Success(logs, "Logs retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Failed to retrieve all logs from MongoDB", Error = ex.Message });
        }
    }

    [HttpGet("logsByListingCase/{id}")]
    public async Task<IActionResult> GetLogsByListingCaseId(string id)
    {
        try
        {
            ICollection<ListingCaseLog> logs = await _listingCasesLogRepository.GetLogsByListingCaseId(id);
            return Ok(ApiResponse<ICollection<ListingCaseLog>>.Success(logs, "Logs by listingCases retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Failed to retrieve logs by listing case ID from MongoDB", Error = ex.Message });
        }
    }

    [HttpGet("logsByCreator/{id}")]
    public async Task<IActionResult> GetLogsByCreatorId(string id)
    {
        try
        {
            ICollection<ListingCaseLog> logs = await _listingCasesLogRepository.GetLogsByCreatorId(id);
            return Ok(ApiResponse<ICollection<ListingCaseLog>>.Success(logs, "Logs by creator retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Failed to retrieve logs by creator ID from MongoDB", Error = ex.Message });
        }
    }
    [HttpGet("logsByChanger/{id}")]
    public async Task<IActionResult> GetLogsByChangerId(string id)
    {
        try
        {
            ICollection<ListingCaseLog> logs = await _listingCasesLogRepository.GetLogsByChangerId(id);
            return Ok(ApiResponse<ICollection<ListingCaseLog>>.Success(logs, "Logs by changer retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Failed to retrieve logs by changer ID from MongoDB", Error = ex.Message });
        }
    }

}