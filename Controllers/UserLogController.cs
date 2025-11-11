using Microsoft.AspNetCore.Mvc;
using RecamSystemApi.Utility;

[Route("api/[controller]")]
[ApiController]
public class UserLogController : ControllerBase
{

    private readonly IUserLogRepository _userLogRepository;

    public UserLogController( IUserLogRepository userLogRepository)
    {
        _userLogRepository = userLogRepository;
    }

    [HttpGet("allLogs")]
    public async Task<IActionResult> GetAllLogs()
    {
        try
        {
            ICollection<UserLog> logs = await _userLogRepository.GetAllUsersLog();


            return Ok(ApiResponse<ICollection<UserLog>>.Success(logs, "Logs retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Failed to retrieve all logs from MongoDB", Error = ex.Message });
        }
    }

    [HttpGet("logsByUser/{id}")]
    public async Task<IActionResult> GetLogsByUser(string id)
    {
        try
        {
            ICollection<UserLog> logs = await _userLogRepository.GetLogsByUserId(id);
            return Ok(ApiResponse<ICollection<UserLog>>.Success(logs, "Logs by user id retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Failed to retrieve logs by User ID from MongoDB", Error = ex.Message });
        }
    }



}