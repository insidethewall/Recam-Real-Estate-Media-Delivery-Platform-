public interface IUserProfileDto
{
    public string CompanyName { get; set; }
    public string FirstName { get; set; } 
    public string LastName { get; set; }
    public string? AvatarUrl { get; set; }
}