using System;
using Microsoft.AspNetCore.Authorization;
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

    public ListingCaseController(IListingCasesService service)
    {
        _service = service;
    }

    [Authorize(Roles = "Admin, Photographer")]
    [HttpPost("createlistingcase")]
    public async Task<IActionResult> CreateListingCase([FromBody] ListingCaseDto listingCaseDto)
    {
        string? currentUserId = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized("Current user ID not found.");
        }
        if (listingCaseDto == null)
        {
            return BadRequest("Listing case cannot be null.");
        }

        ApiResponse<object?> response = await _service.CreateListingCaseAsync(listingCaseDto, currentUserId);
        return response.Succeed
            ? StatusCode(201, response.Data)
            : BadRequest(response.ErrorMessage);
    }

    [Authorize(Roles = "Admin, Photographer")]
    [HttpPut("update/{listingcaseId}")]
    public async Task<IActionResult> UpdateListingCase([FromBody] UpdateListingCaseDto listingCaseDto, [FromRoute] string listingcaseId) { 
        if (listingCaseDto == null)
        {
            return BadRequest("Listing case cannot be null.");
        }
        if (string.IsNullOrEmpty(listingcaseId))
        {
            return BadRequest("Listing case ID cannot be null or empty.");
        }

        ApiResponse<UpdateListingCaseDto?> response = await _service.UpdateListingCaseAsync(listingCaseDto, listingcaseId);
        return response.Succeed
            ? Ok(response.Data)
            : BadRequest(response.ErrorMessage);
    }    

    [Authorize(Roles = "Admin, Photographer")]
    [HttpPatch("{listingcaseId}/status")]
    public async Task<IActionResult> ChangeListingCaseStatus([FromBody] ListcaseStatus listingCaseStatus, [FromRoute] string listingcaseId) { 
        if (string.IsNullOrEmpty(listingcaseId))
        {
            return BadRequest("Listing case ID cannot be null or empty.");
        }

        ApiResponse<ListingCaseStatusDto?> response = await _service.ChangeListingCaseStatusAsync(listingCaseStatus, listingcaseId);
        return response.Succeed
            ? Ok(response.Data)
            : BadRequest(response.ErrorMessage);
    }    

    [Authorize(Roles = "Admin, Photographer")]
    [HttpPost("addAgentsToListingCase/{listingCaseId}")]
    public async Task<IActionResult> AddAgentsToListingCase([FromBody] ICollection<string> agentIds, [FromRoute] string listingCaseId)
    {
        if (agentIds == null || !agentIds.Any())
        {
            return BadRequest("Agents IDs cannot be null or empty.");
        }
        if (string.IsNullOrEmpty(listingCaseId))
        {
            return BadRequest("Listing case ID cannot be null or empty.");
        }

        ApiResponse<object?> response = await _service.AddAgentsToListingCaseAsync(agentIds, listingCaseId);
        return response.Succeed
            ? Ok(response.Data)
            : BadRequest(response.ErrorMessage);
    }

    [Authorize(Roles = "Agent")]
    [HttpGet("getListingCasesByAgent/{userId}")]
    public async Task<IActionResult> GetAllListingCasesByAgentAsync([FromRoute] string userId)
    {
            ApiResponse<ICollection<ListingCase>> response = await _service.GetAllListingCasesByAgentAsync(userId);
            return response.Succeed
            ? Ok(response.Data)
            : BadRequest(response.ErrorMessage);
    } 

    [Authorize(Roles = "Admin, Photographer")]
    [HttpGet("getListingCasesByCreator/{userId}")]
     public async Task<IActionResult> GetAllListingCasesByCreatorAsync([FromRoute] string userId)
    {
            ApiResponse<ICollection<ListingCase>> response = await _service.GetAllListingCasesByCreatorAsync(userId);
            return response.Succeed
            ? Ok(response.Data)
            : BadRequest(response.ErrorMessage);
    } 

    [Authorize(Roles = "Admin, Photographer")]
    [HttpDelete("deleteListingCase/{listingCaseId}")]
    public async Task<IActionResult> DeleteListingCase([FromRoute] string listingCaseId)
    {
        if (string.IsNullOrEmpty(listingCaseId))
        {
            return BadRequest("Listing case ID cannot be null or empty.");
        }

        ApiResponse<ListingCase> response = await _service.DeleteListingCaseAsync(listingCaseId);
        return response.Succeed
            ? Ok(response.Data)
            : BadRequest(response.ErrorMessage);
    }

    [HttpGet]
    public async Task<ActionResult<ICollection<ListingCase>>> GetAllListingCasesAsync()
    {
        ICollection<ListingCase> listingCases = await _service.GetAllListingCasesAsync();
        return Ok(listingCases);
        
    } 



}