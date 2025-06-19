using Microsoft.EntityFrameworkCore;
using RecamSystemApi.Data;
using RecamSystemApi.Enums;
using RecamSystemApi.Models;


public class AuthRepository : IAuthRepository
{
    private readonly ReacmDbContext _context;

    public AuthRepository(ReacmDbContext context)
    {
        _context = context;
    }

    public async Task CreatePhotographerAsync(IUserProfileDto photographerDto, User user)
    {
        Photographer photographer = new Photographer
        {
            Id = user.Id,
            User = user,
            CompanyName = photographerDto.CompanyName,
            PhotographerFirstName = photographerDto.FirstName,
            PhotographerLastName = photographerDto.LastName,
            AvatarUrl = photographerDto.AvatarUrl
        };
        await _context.PhotographyCompanies.AddAsync(photographer);
        await _context.SaveChangesAsync();
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