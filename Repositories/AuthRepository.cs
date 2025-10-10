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

}