namespace RecamSystemApi.DTOs;

public class AgentDto : IUserProfileDto
{

    public required string CompanyName { get; set; } 
    public required string FirstName { get; set; } 
    public required string LastName { get; set; } 
    public string? AvatarUrl { get; set; }
}