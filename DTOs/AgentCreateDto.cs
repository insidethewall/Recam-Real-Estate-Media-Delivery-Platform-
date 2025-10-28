namespace RecamSystemApi.DTOs;

public class AgentCreateDto : IUserProfileDto
{
    public required string Email { get; set; }
    public required string CompanyName { get; set; } 
    public required string FirstName { get; set; } 
    public required string LastName { get; set; } 
    public IFormFile? Avatar { get; set; }
}