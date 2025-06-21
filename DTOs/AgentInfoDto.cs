namespace RecamSystemApi.DTOs;

public class UserInfoDto : IUserProfileDto
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string CompanyName { get; set; } 
    public required string FirstName { get; set; } 
    public required string LastName { get; set; } 
    public string? AvatarUrl { get; set; }
}