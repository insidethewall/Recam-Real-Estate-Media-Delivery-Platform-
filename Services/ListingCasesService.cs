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
    private readonly LoggingContextService _loggingContext;
    private readonly ILogger<ListingCasesService> _logger;

    public ListingCasesService(IGeneralRepository generalRepository, IListingCasesRepository repository, UserManager<User> userManager, IAgentListingCaseValidator validator, LoggingContextService loggingContext, ILogger<ListingCasesService> logger)
    {
        _generalRepository = generalRepository;
        _repository = repository;
        _userManager = userManager;
        _validator = validator;
        _loggingContext = loggingContext;
        _logger = logger;

    }

  

    public async Task<ListingCaseDto> CreateListingCaseAsync(ListingCaseDto listingCaseDto, User currentUser)
    {
        ListingCase listingCase = _generalRepository.MapDto<ListingCaseDto, ListingCase>(listingCaseDto);
        listingCase.UserId = currentUser.Id;
        await _repository.AddListingCaseAsync(listingCase);
        await _generalRepository.SaveChangesAsync();

        // Set logging context - filter will handle persisting the log
        _loggingContext.SetAfterListingCase(listingCase);
        _loggingContext.SetCurrentUser(currentUser);

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
        List<FieldChange> changes = ListingCaseDiff.Diff(before, after);
        await _generalRepository.SaveChangesAsync();

        // Set logging context - filter will handle persisting the log
        _loggingContext.SetBeforeListingCase(before);
        _loggingContext.SetAfterListingCase(after);
        _loggingContext.SetCurrentUser(before.User ?? throw new Exception("creator of listingcase log cannot be null"));
        _loggingContext.SetFieldChanges(changes);

        return listingCaseDto;
    }

    public async Task<ListingCaseStatusDto> ChangeListingCaseStatusAsync(ListcaseStatus newStatus, string listingCaseId)
    {
        ListingCase listingCase = await _validator.ValidateListingCaseAsync(listingCaseId);

        // Snapshot BEFORE
        var before = _generalRepository.MapDto<ListingCase, ListingCase>(listingCase);

        // Apply status change
        listingCase.ListcaseStatus = newStatus;
        List<FieldChange> changes = ListingCaseDiff.Diff(before, listingCase);
        await _generalRepository.SaveChangesAsync();

        // Set logging context - filter will handle persisting the log
        _loggingContext.SetBeforeListingCase(before);
        _loggingContext.SetAfterListingCase(listingCase);
        _loggingContext.SetCurrentUser(before.User ?? throw new Exception("creator of listing case log cannot be null"));
        _loggingContext.SetFieldChanges(changes);

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
            List<FieldChange> fieldChanges = new List<FieldChange>();

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
                FieldChange fieldChange = new FieldChange("AgentListingCase[+]", "null", agent.Id);
                fieldChanges.Add(fieldChange);
                await _repository.AddAgentListingCaseAsync(agentListingCase);
                agentListingCases.Add(agentListingCase);

            }
            await _generalRepository.SaveChangesAsync();

            // Set logging context - filter will handle persisting the log
            _loggingContext.SetAfterListingCase(listingCase);
            _loggingContext.SetCurrentUser(listingCase.User ?? throw new Exception("creator of listing case log cannot be null"));
            _loggingContext.SetFieldChanges(fieldChanges);

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
        List<FieldChange> fieldChanges = new List<FieldChange>();
        try
        {
            ListingCase listingCase = await _validator.ValidateListingCaseAsync(listingCaseId);
            var agentListingCases = new List<AgentListingCase>();

            foreach (string agentId in agentIds)
            {
                bool exist = await _validator.ValidateAgentAndListingCaseAsync(agentId, listingCaseId);
                if (!exist)
                    throw new Exception($"Agent with ID {agentId} does not associate with listing case ID {listingCaseId}.");
                AgentListingCase deleted = await _repository.RemoveAgentListingCaseAsync(agentId, listingCaseId);
                agentListingCases.Add(deleted);
                FieldChange fieldChange = new FieldChange("AgentListingCase[-]", agentId, "null");
                fieldChanges.Add(fieldChange);
            }
            await _generalRepository.SaveChangesAsync();

            // Set logging context - filter will handle persisting the log
            _loggingContext.SetAfterListingCase(listingCase);
            _loggingContext.SetCurrentUser(listingCase.User ?? throw new Exception("creator of listing case log cannot be null"));
            _loggingContext.SetFieldChanges(fieldChanges);

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

            // Set logging context - filter will handle persisting the log
            _loggingContext.SetBeforeListingCase(listingCase);
            _loggingContext.SetCurrentUser(listingCase.User ?? throw new Exception("creator of listing case log cannot be null"));

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
        await _validator.ValidateListingCaseAsync(listingCaseId);
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