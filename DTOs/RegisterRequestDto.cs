using RecamSystemApi.Enums;

namespace RecamSystemApi.DTOs;

public class RegisterRequestDto
{
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public Role Role { get; set; }
}

