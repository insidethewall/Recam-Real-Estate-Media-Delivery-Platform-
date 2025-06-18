using RecamSystemApi.Data;
using RecamSystemApi.Enums;
using RecamSystemApi.Models;


public class AuthRepository: IAuthRepository
{
    private readonly ReacmDbContext _context;

    public AuthRepository(ReacmDbContext context)
    {
        _context = context;
    }

    public async Task AddUserProfileAsync(Role role, IUserProfileDto userProfile, User user)
    {
        switch (role)
        {
            case Role.Admin:
                throw new ArgumentException("Admin profile creation is not supported through this method.");
                
            case Role.Photographer:
                var photographer = new Photographer
                {
                    Id = user.Id,
                    User = user,
                    CompanyName = userProfile.CompanyName,
                    PhotographerFirstName = userProfile.FirstName,
                    PhotographerLastName = userProfile.LastName,
                    AvatarUrl = userProfile.AvatarUrl
                };
                await _context.PhotographyCompanies.AddAsync(photographer);
                await _context.SaveChangesAsync();
                break;
            case Role.Agent:
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
                break;
            default:
                throw new ArgumentException("Invalid role for user profile creation.");
        }
    }

    // public async Task<ApiResponse<object?>> DeleteUserProfileAsync(string userId, Role role)
    // {
    //     switch (role)
    //     {
    //         case Role.Admin:
    //             throw new ArgumentException("Admin profile deletion is not supported through this method.");
                
    //         case Role.Photographer:
    //             Photographer? photographer = await _context.PhotographyCompanies.Include(a => a.User).FirstOrDefaultAsync(a => a.Id == userId);
    //             if (photographer != null)
    //             {
    //                 photographer.User.IsDeleted = true; // Soft delete
    //                 await _context.SaveChangesAsync();
    //                 return ApiResponse<object?>.Success(photographer, $"Photographer profile {photographer.Id} Soft deleted successfully.");
    //             }
    //                 return ApiResponse<object?>.Fail($"Photographer profile with ID {userId} not found.", "404");
    //         case Role.Agent:
    //             Agent? agent = await _context.Agents.Include(a => a.User).FirstOrDefaultAsync(a => a.Id == userId);
    //             if (agent != null)
    //             {
    //                 agent.User.IsDeleted = true;
    //                 await _context.SaveChangesAsync();
    //                 return ApiResponse<object?>.Success(agent, $"Agent profile {agent.Id} Soft deleted successfully.");
    //             }
    //             return ApiResponse<object?>.Fail($"Agent profile with ID {userId} not found.", "404");
           
    //         default:
    //             return ApiResponse<object?>.Fail("Invalid role for user profile deletion.");
    //     }
    // }

}