using RecamSystemApi.Data;
using RecamSystemApi.DTOs;
using RecamSystemApi.Models;

public class AuthRepository: IAuthRepository
{
    private readonly ReacmDbContext _context;

    public AuthRepository(ReacmDbContext context)
    {
        _context = context;
    }

    public async Task AddUserProfileAsync(IUserProfileDto userProfile, User user)
    {
        var agent = new Agent
        {
            Id = user.Id,
            User = user,
            CompanyName = userProfile.CompanyName,
            AgentFirstName = userProfile.FirstName,
            AgentLastName = userProfile.LastName,
            AvatarUrl = userProfile.AvatarUrl
        };

        await _context.Agents.AddAsync(agent);
        await _context.SaveChangesAsync();
    }

}