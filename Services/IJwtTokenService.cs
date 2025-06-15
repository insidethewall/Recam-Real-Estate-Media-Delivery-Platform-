using RecamSystemApi.Models;

public interface IJwtTokenService
{
    Task<string> GenerateTokenAsync(User user);
}