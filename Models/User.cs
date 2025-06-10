using Microsoft.AspNetCore.Identity;
using RecamSystemApi.Enums;

namespace RecamSystemApi.Models;

public class User : IdentityUser
{
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
}
