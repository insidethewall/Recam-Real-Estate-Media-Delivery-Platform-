using Microsoft.AspNetCore.Mvc;
using RecamSystemApi.Models;
using RecamSystemApi.Services;

namespace RecamSystemApi.Controllers;

[ApiController]
[Route("api/listingCases")]
public class ListingCaseController : ControllerBase
{
    private readonly IListingCasesService _service;

    public ListingCaseController(IListingCasesService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ICollection<ListingCase>>> GetAllListingCasesAsync()
    {
        ICollection<ListingCase> listingCases = await _service.GetAllListingCasesAsync();
        return Ok(listingCases);
        
    } 



}