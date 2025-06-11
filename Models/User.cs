using Microsoft.AspNetCore.Identity;


namespace RecamSystemApi.Models;

public class User : IdentityUser
{
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
}
