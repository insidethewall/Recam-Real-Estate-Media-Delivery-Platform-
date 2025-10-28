using Microsoft.EntityFrameworkCore;
using RecamSystemApi.Data;
using RecamSystemApi.DTOs;
using RecamSystemApi.Models;

public class UserRepository : IUserRepository
{
    private readonly ReacmDbContext _context;

    public UserRepository(ReacmDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
    }

    public async Task CreateAgentAsync(IUserProfileDto userProfile, User user)
    {
        var agent = new Agent
        {
            Id = user.Id,
            User = user,
            CompanyName = userProfile.CompanyName,
            AgentFirstName = userProfile.FirstName,
            AgentLastName = userProfile.LastName,
            AvatarUrl = userProfile.AvatarUrl,
        };
        await _context.Agents.AddAsync(agent);
        await _context.SaveChangesAsync();
    }
    public async Task<AgentPhotographer> CreateAgentPhotographerAsync(User currentUser, User agentUser)
    {
        Photographer? photographer = await _context.PhotographyCompanies.FindAsync(currentUser.Id);
        Agent? agent = await _context.Agents.FindAsync(agentUser.Id);

        if (photographer == null || agent == null)
        {
            throw new ArgumentException("Photographer or Agent not found.");
        }

        AgentPhotographer agentPhotographer = new AgentPhotographer
        {
            PhotographerId = photographer.Id,
            AgentId = agent.Id,
            Photographer = photographer,
            Agent = agent
        };
        await _context.AgentPhotographers.AddAsync(agentPhotographer);
        await _context.SaveChangesAsync();
        return agentPhotographer;

    }

    public async Task<ICollection<AgentPhotographer>> DeleteAgentPhotographerCompany(User user)
    {
        ICollection<AgentPhotographer> agentPhotographers = await _context.AgentPhotographers
            .Where(ap => ap.PhotographerId == user.Id || ap.AgentId == user.Id)
            .ToListAsync();
        _context.AgentPhotographers.RemoveRange(agentPhotographers);
        await _context.SaveChangesAsync();
        return agentPhotographers;

    }

    public async Task<Photographer> DeletePhotographerAsync(string userId)
    {
        try
        {
            Photographer? photographer = await _context.PhotographyCompanies
                .FirstOrDefaultAsync(p => p.Id == userId);
            if (photographer == null)
            {
                throw new Exception($"Photographer with ID {userId} not found.");
            }
            _context.PhotographyCompanies.Remove(photographer);
            await _context.SaveChangesAsync();
            return photographer;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deleting PhotographerCompany: {ex.Message}");
        }
    }

    public async Task<ICollection<UserInfoDto>> GetAllAgentsAsync()
    {
        return await _context.Agents.Where(agent => agent.User.IsDeleted == false).Select(agent => new UserInfoDto
        {
            Id = agent.Id,
            Email = agent.User.Email ?? string.Empty,
            CompanyName = agent.CompanyName,
            FirstName = agent.AgentFirstName,
            LastName = agent.AgentLastName,
            AvatarUrl = agent.AvatarUrl
        }).ToListAsync();
    }

    public async Task<ICollection<UserInfoDto>> GetAllPhotographersAsync()
    {
        return await _context.PhotographyCompanies.Where(p => p.User.IsDeleted == false).Select(p => new UserInfoDto
        {
            Id = p.Id,
            Email = p.User.Email ?? string.Empty,
            CompanyName = p.CompanyName,
            FirstName = p.PhotographerFirstName,
            LastName = p.PhotographerLastName,
            AvatarUrl = p.AvatarUrl
        }).ToListAsync();
    }

    public async Task<ICollection<UserInfoDto>> GetAgentsByPhotographerAsync(string photographerId)
    {
        if (string.IsNullOrEmpty(photographerId))
            throw new ArgumentException("Photographer ID cannot be null or empty.", nameof(photographerId));
        return await _context.AgentPhotographers
            .Where(ap => ap.PhotographerId == photographerId && ap.Agent.User.IsDeleted == false)
            .Select(ap => new UserInfoDto
            {
                Id = ap.Agent.Id,
                Email = ap.Agent.User.Email ?? string.Empty,
                CompanyName = ap.Agent.CompanyName,
                FirstName = ap.Agent.AgentFirstName,
                LastName = ap.Agent.AgentLastName,
                AvatarUrl = ap.Agent.AvatarUrl
            }).ToListAsync();
    }

    public Photographer DeletePhotographer(User user)
    {
        Photographer? photographer = _context.PhotographyCompanies.Include(p=>p.AgentPhotographer)
            .FirstOrDefault(p => p.Id == user.Id);

        if (photographer == null)
        {
            throw new ArgumentException("Photographer not found.");
        }
        _context.PhotographyCompanies.Remove(photographer);
        return photographer;
    }

    public Agent DeleteAgent(User user)
    {
        Agent? agent = _context.Agents.Include(a=>a.AgentPhotographer).Include(a=>a.AgentListingCases)
            .FirstOrDefault(a => a.Id == user.Id);

        if (agent == null)
        {
            throw new ArgumentException("Agent not found.");
        }
        _context.Agents.Remove(agent);
        return agent;
    }   
    

}