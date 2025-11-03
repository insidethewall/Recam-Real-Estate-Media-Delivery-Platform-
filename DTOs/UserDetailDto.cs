using RecamSystemApi.Enums;

namespace RecamSystemApi.DTOs;

public class UserDetailDto
{

    public required string UserId { get; set; }
   public required string UserName { get; set; }
    
    public Role Role { get; set; }
   
}