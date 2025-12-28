using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RecamSystemApi.Attributes;
using RecamSystemApi.Services;
using RecamSystemApi.Helper;

namespace RecamSystemApi.Filters;

/// <summary>
/// Action filter that automatically logs listing case changes to MongoDB.
/// Services should use LoggingContextService to set before/after state and field changes,
/// and this filter will create the log entry after the action completes successfully.
/// </summary>
public class ListingCaseActionLoggingFilter : IAsyncActionFilter
{
    private readonly IListingCasesLogRepository _listingCasesLogRepository;
    private readonly LoggingContextService _loggingContext;
    private readonly ILogger<ListingCaseActionLoggingFilter> _logger;

    public ListingCaseActionLoggingFilter(
        IListingCasesLogRepository listingCasesLogRepository,
        LoggingContextService loggingContext,
        ILogger<ListingCaseActionLoggingFilter> logger)
    {
        _listingCasesLogRepository = listingCasesLogRepository;
        _loggingContext = loggingContext;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Check if the action has the LogListingCaseAction attribute
        var logAttribute = context.ActionDescriptor.EndpointMetadata
            .OfType<LogListingCaseActionAttribute>()
            .FirstOrDefault();

        if (logAttribute == null)
        {
            // No logging needed, just continue
            await next();
            return;
        }

        // Execute the action first - service will populate the logging context
        var executedContext = await next();

        // Only log if the action was successful
        if (executedContext.Exception == null && IsSuccessResult(executedContext.Result))
        {
            try
            {
                var listingCase = logAttribute.ChangeType == ChangeType.Created
                    ? _loggingContext.GetAfterListingCase()
                    : _loggingContext.GetBeforeListingCase();

                var currentUser = _loggingContext.GetCurrentUser();
                var changer = logAttribute.ChangeType == ChangeType.Updated
                    ? _loggingContext.GetAfterListingCase()?.User
                    : null;

                if (listingCase == null)
                {
                    _logger.LogWarning($"Cannot log listing case action {logAttribute.ChangeType}: listing case not set in LoggingContext");
                    return;
                }

                if (currentUser == null)
                {
                    // Fallback to listing case creator
                    currentUser = listingCase.User ?? throw new System.Exception("Cannot determine current user for listing case log");
                }

                // Get field changes if applicable
                List<FieldChange>? changes = null;
                if (logAttribute.CaptureFieldChanges && logAttribute.ChangeType == ChangeType.Updated)
                {
                    changes = _loggingContext.GetFieldChanges();

                    // If not set in context, try to calculate from before/after
                    if (changes == null)
                    {
                        var before = _loggingContext.GetBeforeListingCase();
                        var after = _loggingContext.GetAfterListingCase();
                        if (before != null && after != null)
                        {
                            changes = ListingCaseDiff.Diff(before, after);
                        }
                    }
                }

                // Create and save the log
                var listingCaseLog = await _listingCasesLogRepository.CreateListingCaseLog(
                    listingCase: listingCase,
                    changeType: logAttribute.ChangeType,
                    creator: currentUser,
                    changer: changer,
                    info: logAttribute.AdditionalInfo,
                    changes: changes
                );

                await _listingCasesLogRepository.AddLog(listingCaseLog);
                _logger.LogInformation($"Listing case action logged: {logAttribute.ChangeType} for case {listingCase.Id} by user {currentUser.Email}");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Failed to log listing case action: {logAttribute.ChangeType}");
                // Don't throw - logging failure shouldn't break the request
            }
            finally
            {
                _loggingContext.Clear();
            }
        }
        else
        {
            // Clear context on failure
            _loggingContext.Clear();
        }
    }

    /// <summary>
    /// Checks if the action result indicates success
    /// </summary>
    private bool IsSuccessResult(IActionResult? result)
    {
        return result is OkObjectResult or OkResult or CreatedResult or CreatedAtActionResult or NoContentResult;
    }
}
