using Microsoft.EntityFrameworkCore;
using RecamSystemApi.Data;
using RecamSystemApi.DTOs;
using RecamSystemApi.Models;

public class UserRepository : IUserRepository
{
    private readonly ReacmDbContext _context;

    public UserRepository(ReacmDbContext context)
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
    public async Task CreateAgentPhotographerAsync(string photographerId, string agentId)
    {
        Photographer? photographer = await _context.PhotographyCompanies.FindAsync(photographerId);
        Agent? agent = await _context.Agents.FindAsync(agentId);

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
        try
        {
            await _context.AgentPhotographers.AddAsync(agentPhotographer);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error adding AgentPhotographer: {ex.Message}");

        }

    }

    public async Task DeleteAgentPhotographerCompany(string userId)
    {
        try
        {
            List<AgentPhotographer> agentPhotographers = await _context.AgentPhotographers
                .Where(ap => ap.PhotographerId == userId || ap.AgentId == userId)
                .ToListAsync();
            _context.AgentPhotographers.RemoveRange(agentPhotographers);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deleting AgentPhotographerCompany: {ex.Message}");
        }
    }

    public async Task<ICollection<Agent>> GetAllAgentsAsync()
    {
        return await _context.Agents.ToListAsync();
    }

    public async Task<ICollection<Photographer>> GetAllPhotographersAsync()
    {
        return await _context.PhotographyCompanies.ToListAsync();
    }

    public async Task<ICollection<AgentInfoDto>> GetAgentsByPhotographerAsync(string photographerId)
    {
        if (string.IsNullOrEmpty(photographerId))
            throw new ArgumentException("Photographer ID cannot be null or empty.", nameof(photographerId));
        return await _context.AgentPhotographers
            .Where(ap => ap.PhotographerId == photographerId && ap.Agent.User.IsDeleted == false)
            .Select(ap=> new AgentInfoDto
            {
                Id = ap.Agent.Id,
                Email = ap.Agent.User.Email ?? string.Empty,
                CompanyName = ap.Agent.CompanyName,
                FirstName = ap.Agent.AgentFirstName,
                LastName = ap.Agent.AgentLastName,
                AvatarUrl = ap.Agent.AvatarUrl
             }).ToListAsync();
    }
    

}