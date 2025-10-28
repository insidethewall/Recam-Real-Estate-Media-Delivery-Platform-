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

    public async Task CreatePhotographerAsync(Photographer photographer)
    {
 
        await _context.PhotographyCompanies.AddAsync(photographer);
    
    }

}