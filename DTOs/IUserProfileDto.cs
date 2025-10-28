using Microsoft.AspNetCore.Http;

public interface IUserProfileDto
{
    string CompanyName { get; set; }
    string FirstName { get; set; } 
    string LastName { get; set; }
    IFormFile? Avatar { get; set; }
}