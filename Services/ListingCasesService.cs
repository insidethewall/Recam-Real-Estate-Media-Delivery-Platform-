using Microsoft.AspNetCore.Identity;
using RecamSystemApi.DTOs;
using RecamSystemApi.Enums;
using RecamSystemApi.Models;
using RecamSystemApi.Services;

public class ListingCasesService : IListingCasesService
{
    private readonly IGeneralRepository _generalRepository;
    private readonly IListingCasesRepository _repository;
    private readonly UserManager<User> _userManager;
    private readonly IAgentListingCaseValidator _validator;
    public ListingCasesService(IGeneralRepository generalRepository, IListingCasesRepository repository, UserManager<User> userManager, IAgentListingCaseValidator validator)
    {
        _generalRepository = generalRepository;
        _repository = repository;
        _userManager = userManager;
        _validator = validator;
    }

    public async Task<ICollection<ListingCaseWithNavDto>> GetAllListingCasesAsync()
    {
        return await _repository.GetAllListingCasesAsync();
    }

    public async Task<ListingCaseDto> CreateListingCaseAsync(ListingCaseDto listingCaseDto, User currentUser)
    {
        ListingCase listingCase = _generalRepository.MapDto<ListingCaseDto, ListingCase>(listingCaseDto);
        listingCase.UserId = currentUser.Id;
        await _repository.AddListingCaseAsync(listingCase);
        await _generalRepository.SaveChangesAsync();
        return listingCaseDto;

    }

// just basic update, no references update
    public async Task<UpdateListingCaseDto> UpdateListingCaseAsync(UpdateListingCaseDto listingCaseDto, string listingCaseId)
    {
            ListingCase existingListingCase = await _repository.GetListingCaseByIdAsync(listingCaseId);
            _generalRepository.MapDtoUpdate(listingCaseDto, existingListingCase);
            await _generalRepository.SaveChangesAsync();
            return listingCaseDto;
    }

    public async Task<ListingCaseStatusDto> ChangeListingCaseStatusAsync(ListcaseStatus newStatus, string listingCaseId)
    {
        
        ListingCase listingCase = await _validator.ValidateListingCaseAsync(listingCaseId);

        listingCase.ListcaseStatus = newStatus;

        await _generalRepository.SaveChangesAsync();
        ListingCaseStatusDto statusDto = new ListingCaseStatusDto
        {
            Id = listingCase.Id,
            title = listingCase.Title,
            Status = listingCase.ListcaseStatus
        };
        return statusDto;

    }
    
    public async Task<List<AgentListingCase>> AddAgentsToListingCaseAsync(ICollection<string> agentIds, string listingCaseId)
    {

        await using var transaction = await _generalRepository.BeginTransactionAsync();
        try
        {
            ListingCase listingCase = await _validator.ValidateListingCaseAsync(listingCaseId);
            var agentListingCases = new List<AgentListingCase>();

            foreach (string agentId in agentIds)
            {
                await _validator.ValidateAgentAndListingCaseAsync(agentId, listingCaseId);
                User user = await _validator.ValidateUserByRoleAsync(agentId, Role.Agent);
                Agent agent = user.Agent!;
                AgentListingCase agentListingCase = new AgentListingCase
                {
                    AgentId = agent.Id,
                    ListingCaseId = listingCase.Id,
                    Agent = agent,
                    ListingCase = listingCase
                };
                await _repository.AddAgentListingCaseAsync(agentListingCase);
                await _generalRepository.SaveChangesAsync();
                agentListingCases.Add(agentListingCase);

            }
            await transaction.CommitAsync();
            return agentListingCases;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"Error adding agents to listing case: {ex.Message}");
        }
    }

    public async Task<ICollection<ListingCase>> GetAllListingCasesByAgentAsync(string currentUserId)
    {
        User user = await _validator.ValidateUserByRoleAsync(currentUserId, Role.Agent);
        Agent agent = user.Agent!;
        ICollection<ListingCase> listingCases =  _repository.GetAllListingCasesByAgentAsync(agent);
        return listingCases;
    }

    public async Task<ICollection<ListingCase>> GetAllListingCasesByCreatorAsync(User currentUser)
    {
        ICollection<ListingCase> listingCases = await _repository.GetAllListingCasesByUserAsync(currentUser);
        return listingCases;
    }

    public async Task<ListingCase> DeleteListingCaseAsync(string listingCaseId)
    {
        await using var transaction = await _generalRepository.BeginTransactionAsync();
        try
        {
            ListingCase listingCase = await _validator.ValidateListingCaseAsync(listingCaseId);
            _repository.DeleteListingCase(listingCase);
            _repository.DeleteAgentListingCase(listingCase);
            _repository.RemoveListingCaseFromUser(listingCase);
            await _generalRepository.SaveChangesAsync();
            return listingCase;

        }
           catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"Error deleting listingCases: {ex.Message}");
        }

    }
     

}