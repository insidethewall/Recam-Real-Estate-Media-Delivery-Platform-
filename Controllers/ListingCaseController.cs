using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecamSystemApi.DTOs;
using RecamSystemApi.Models;
using RecamSystemApi.Services;
using RecamSystemApi.Utility;

namespace RecamSystemApi.Controllers;

[ApiController]
[Route("api/listingcases")]
public class ListingCaseController : ControllerBase
{
    private readonly IListingCasesService _service;

    private readonly UserManager<User> _userManager;

    public ListingCaseController(IListingCasesService service, UserManager<User> userManager)
    {
        _service = service;
        _userManager = userManager;
    }


    [Authorize(Roles = "Admin, Photographer")]
    [HttpPost("createlistingcase")]
    public async Task<IActionResult> CreateListingCase([FromBody] ListingCaseDto listingCaseDto)
    {
        try
        { 
            string? currentUserId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(ApiResponse<string>.Fail("User ID not found in token.", "401"));
            }
            User? currentUser = await _userManager.FindByIdAsync(currentUserId);
            if (currentUser == null)
            {
                return Unauthorized(ApiResponse<object?>.Fail("Current user not found.", "401"));
            }
            if(currentUser.IsDeleted)
            {
                return Unauthorized(ApiResponse<string>.Fail("User account is deleted.", "401"));
            }
            if (listingCaseDto == null)
            {
                return BadRequest(ApiResponse<string>.Fail("Listing case cannot be null.", "400"));
            }

            ListingCaseDto response = await _service.CreateListingCaseAsync(listingCaseDto, currentUser);
            return StatusCode(201, ApiResponse<ListingCaseDto>.Success(response));
            
        } catch (System.Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error creating listing case: {ex.Message}", "500"));
        }

          
    }

    [Authorize(Roles = "Admin, Photographer")]
    [HttpPut("update/{listingcaseId}")]
    public async Task<IActionResult> UpdateListingCase([FromBody] UpdateListingCaseDto listingCaseDto, [FromRoute] string listingcaseId)
    {
        if (listingCaseDto == null)
        {
            return BadRequest(ApiResponse<string>.Fail("Listing case cannot be null.", "400"));
        }
        if (string.IsNullOrEmpty(listingcaseId))
        {
            return BadRequest(ApiResponse<string>.Fail("Listing case ID cannot be null or empty.", "400"));
        }
        try
        {
            UpdateListingCaseDto response = await _service.UpdateListingCaseAsync(listingCaseDto, listingcaseId);
            return Ok(ApiResponse<UpdateListingCaseDto?>.Success(response, "Listing case updated successfully."));

        }
        catch (InvalidOperationException)
        {
            return NotFound(ApiResponse<UpdateListingCaseDto?>.Fail($"Listing case with ID {listingcaseId} not found.", "404"));
        }
        catch (System.Exception ex)
        {
            return BadRequest(ApiResponse<UpdateListingCaseDto?>.Fail($"Error updating listing case: {ex.Message}", "500"));
        }
    }

    [Authorize(Roles = "Admin, Photographer")]
    [HttpPatch("{listingcaseId}/status")]
    public async Task<IActionResult> ChangeListingCaseStatus([FromBody] ListcaseStatus listingCaseStatus, [FromRoute] string listingcaseId) {
        if (string.IsNullOrEmpty(listingcaseId))
        {
            return BadRequest(ApiResponse<string>.Fail("Listing case ID cannot be null or empty.", "400"));
        }
        try
        {
            ListingCaseStatusDto response = await _service.ChangeListingCaseStatusAsync(listingCaseStatus, listingcaseId);
            return Ok(ApiResponse<ListingCaseStatusDto>.Success(response, "Listing case status updated successfully."));

        }
        catch (InvalidOperationException)
        {
            return NotFound(ApiResponse<string>.Fail($"Listing case with ID {listingcaseId} not found.", "404"));
        }
        catch (System.Exception ex)
        {
            return BadRequest(ApiResponse<string>.Fail($"Error updating listing case status: {ex.Message}", "500"));
        }

   
    }    

    [Authorize(Roles = "Admin, Photographer")]
    [HttpPost("addAgentsToListingCase/{listingCaseId}")]
    public async Task<IActionResult> AddAgentsToListingCase([FromBody] ICollection<string> agentIds, [FromRoute] string listingCaseId)
    {
        if (agentIds == null || !agentIds.Any())
        {
            return BadRequest(ApiResponse<string>.Fail("Agent IDs cannot be null or empty.", "400"));
        }
        if (string.IsNullOrEmpty(listingCaseId))
        {
            return BadRequest(ApiResponse<string>.Fail("Listing case ID cannot be null or empty."));
        }
        try
        {
            List<AgentListingCase> response = await _service.AddAgentsToListingCaseAsync(agentIds, listingCaseId);
               return Ok(ApiResponse<List<AgentListingCase>>.Success(response, "Agents added to listing case successfully."));
            
        }
        catch (System.Exception ex)
        {
            return BadRequest(ApiResponse<string>.Fail($"Error adding agents to listing case: {ex.Message}", "500"));
        }
    }

    [Authorize(Roles = "Agent")]
    [HttpGet("getListingCasesByAgent")]
    public async Task<IActionResult> GetAllListingCasesByAgentAsync()
    {
        string? userId = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<string>.Fail("User ID not found in token.", "401"));
        }
   
        try
        {
            ICollection<ListingCase> response = await _service.GetAllListingCasesByAgentAsync(userId);
            return Ok(ApiResponse<ICollection<ListingCase>>.Success(response, "Listing cases retrieved successfully."));
        }
        catch (System.Exception ex)
        {
            return BadRequest(ApiResponse<string>.Fail($"Error retrieving listing cases: {ex.Message}", "500"));
        }
     
            
    }

    [Authorize(Roles = "Admin, Photographer")]
    [HttpGet("getListingCasesByCreator")]
    public async Task<IActionResult> GetAllListingCasesByCreatorAsync()
    {
        string? userId = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<string>.Fail("User ID not found in token.", "401"));
        }
        User? currentUser = await _userManager.FindByIdAsync(userId);
        
        if (currentUser == null)
        {
            return Unauthorized(ApiResponse<string>.Fail("Current user not found.", "401"));
        }
        if (currentUser.IsDeleted)
        {
            return Unauthorized(ApiResponse<string>.Fail("User account is deleted.", "401"));
        }
        try
        {
            ICollection<ListingCase> response = await _service.GetAllListingCasesByCreatorAsync(currentUser);
            return Ok(ApiResponse<ICollection<ListingCase>>.Success(response, "Listing cases retrieved successfully."));

        }
        catch (System.Exception ex)
        {
            return BadRequest($"Error retrieving listing cases: {ex.Message}");
        }
         
    }

    [Authorize(Roles = "Admin, Photographer")]
    [HttpDelete("deleteListingCase/{listingCaseId}")]
    public async Task<IActionResult> DeleteListingCase([FromRoute] string listingCaseId)
    {
        if (string.IsNullOrEmpty(listingCaseId))
        {
            return BadRequest("Listing case ID cannot be null or empty.");
        }
        try
        {
            ListingCase response = await _service.DeleteListingCaseAsync(listingCaseId);
            return Ok(ApiResponse<ListingCase>.Success(response, "Listing case deleted successfully."));
        }
        catch (System.Exception ex)
        {
            return BadRequest(ApiResponse<string>.Fail($"Error deleting listing case: {ex.Message}", "500"));
        }
 
    }

    [HttpGet]
    public async Task<ActionResult<ICollection<ListingCase>>> GetAllListingCasesAsync()
    {
        ICollection<ListingCase> listingCases = await _service.GetAllListingCasesAsync();
        return Ok(ApiResponse<ICollection<ListingCase>>.Success(listingCases, "Listing cases retrieved successfully."));
        
    } 



}