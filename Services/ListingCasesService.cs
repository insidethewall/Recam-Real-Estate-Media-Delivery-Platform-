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
    private readonly IListingCasesLogRepository _listingCasesLogRepository;

    private readonly ILogger<ListingCasesService> _logger;

    public ListingCasesService(IGeneralRepository generalRepository, IListingCasesRepository repository, UserManager<User> userManager, IAgentListingCaseValidator validator, IListingCasesLogRepository listingCasesLogRepository, ILogger<ListingCasesService> logger)
    {
        _generalRepository = generalRepository;
        _repository = repository;
        _userManager = userManager;
        _validator = validator;
        _listingCasesLogRepository = listingCasesLogRepository;
        _logger = logger;

    }

  

    public async Task<ListingCaseDto> CreateListingCaseAsync(ListingCaseDto listingCaseDto, User currentUser)
    {
        ListingCase listingCase = _generalRepository.MapDto<ListingCaseDto, ListingCase>(listingCaseDto);
        listingCase.UserId = currentUser.Id;
        ListingCaseLog listingCaseLog = await _listingCasesLogRepository.CreateListingCaseLog(listingCase,ChangeType.Created, currentUser);
        await _repository.AddListingCaseAsync(listingCase);
        await _listingCasesLogRepository.AddLog(listingCaseLog);
        await _generalRepository.SaveChangesAsync();
        return listingCaseDto;
    }

    // just basic update, no references update
    public async Task<UpdateListingCaseDto> UpdateListingCaseAsync(UpdateListingCaseDto listingCaseDto, string listingCaseId)
    {
        var existing = await _repository.GetListingCaseByIdAsync(listingCaseId);

        // 1) snapshot BEFORE
        var before = _generalRepository.MapDto<ListingCase, ListingCase>(existing);

        // 2) apply update and CAPTURE the return
        var after = _generalRepository.MapDtoUpdate(listingCaseDto, existing);
        _logger.LogInformation("after==existing? {Eq}", ReferenceEquals(after, existing));
        List<FieldChange> changes = ListingCaseDiff.Diff(before, after);
         ListingCaseLog listingCaseLog = await _listingCasesLogRepository.CreateListingCaseLog(after,ChangeType.Updated, before.User ?? throw new Exception("creator of listingcase log cannot be null"), after.User, "",  changes);
        await _listingCasesLogRepository.AddLog(listingCaseLog);
        await _generalRepository.SaveChangesAsync();

        return listingCaseDto;
    }

    public async Task<ListingCaseStatusDto> ChangeListingCaseStatusAsync(ListcaseStatus newStatus, string listingCaseId)
    {

        ListingCase listingCase = await _validator.ValidateListingCaseAsync(listingCaseId);
        // add to listing case log 
        var before = _generalRepository.MapDto<ListingCase, ListingCase>(listingCase);
        List<FieldChange> changes = ListingCaseDiff.Diff(before, listingCase);
        ListingCaseLog listingCaseLog = await _listingCasesLogRepository.CreateListingCaseLog(listingCase,ChangeType.Updated, before.User ?? throw new Exception("creator of listingcase log cannot be null"), listingCase.User, "",  changes);
        await _listingCasesLogRepository.AddLog(listingCaseLog);

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
                bool exist = await _validator.ValidateAgentAndListingCaseAsync(agentId, listingCaseId);
                if (exist)
                    throw new Exception($"Agent with ID {agentId} is already associated with listing case ID {listingCaseId}.");
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

    public async Task<List<AgentListingCase>> RemoveAgentsFromListingCase(ICollection<string> agentIds, string listingCaseId)
    { 
        await using var transaction = await _generalRepository.BeginTransactionAsync();
        try
        {
            ListingCase listingCase = await _validator.ValidateListingCaseAsync(listingCaseId);
            var agentListingCases = new List<AgentListingCase>();

            foreach (string agentId in agentIds)
            {
                bool exist = await _validator.ValidateAgentAndListingCaseAsync(agentId, listingCaseId);
                if(!exist)
                    throw new Exception($"Agent with ID {agentId} does not associate with listing case ID {listingCaseId}.");
                AgentListingCase deleted = await _repository.RemoveAgentListingCaseAsync(agentId, listingCaseId);
                agentListingCases.Add(deleted);
            }
            await _generalRepository.SaveChangesAsync();
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
        ICollection<ListingCase> listingCases = _repository.GetAllListingCasesByAgentAsync(agent);
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
            _repository.SoftDeleteListingCase(listingCase);
            _repository.DeleteAgentListingCase(listingCase);
            _repository.RemoveListingCaseFromUser(listingCase);
            _repository.SoftDeleteMediaAssetsByListingCase(listingCase);
            await _generalRepository.SaveChangesAsync();
            await transaction.CommitAsync();
            return listingCase;

        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"Error deleting listingCases: {ex.Message}");
        }

    }

    public async Task<ListingCase> GetListingCaseByIdAsync(string listingCaseId)
    {
        ListingCase listingCase = await _validator.ValidateListingCaseAsync(listingCaseId);
        ListingCase SelectedListingCase = await _repository.GetListingCaseByIdAsync(listingCaseId);
        return SelectedListingCase;
    }

    public async Task<ICollection<ListingCaseWithNavDto>> GetAllListingCasesAsync()
    {
        return await _repository.GetAllListingCasesAsync();
    }
    
    public async Task<ICollection<ListingCaseWithNavDto>> GetAllDeletedListingCasesAsync()
    {
        return await _repository.GetAllDeletedListingCasesAsync();
    }
     

}