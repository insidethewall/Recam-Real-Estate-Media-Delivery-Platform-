using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RecamSystemApi.Attributes;
using RecamSystemApi.Services;

namespace RecamSystemApi.Filters;

/// <summary>
/// Action filter that automatically logs user actions to MongoDB.
/// Services should use LoggingContextService to set the log data during execution,
/// and this filter will persist it after the action completes successfully.
/// </summary>
public class UserActionLoggingFilter : IAsyncActionFilter
{
    private readonly IUserLogRepository _userLogRepository;
    private readonly LoggingContextService _loggingContext;
    private readonly ILogger<UserActionLoggingFilter> _logger;

    public UserActionLoggingFilter(
        IUserLogRepository userLogRepository,
        LoggingContextService loggingContext,
        ILogger<UserActionLoggingFilter> logger)
    {
        _userLogRepository = userLogRepository;
        _loggingContext = loggingContext;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Check if the action has the LogUserAction attribute
        var logAttribute = context.ActionDescriptor.EndpointMetadata
            .OfType<LogUserActionAttribute>()
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
                var currentUser = _loggingContext.GetCurrentUser();
                var targetUser = _loggingContext.GetTargetUser();

                // Ensure we have at least the current user
                if (currentUser == null)
                {
                    _logger.LogWarning($"Cannot log user action {logAttribute.Action}: current user not set in LoggingContext");
                    return;
                }

                // Create and save the log
                var userLog = _userLogRepository.CreateUserLog(
                    user: currentUser,
                    action: logAttribute.Action,
                    targetUser: targetUser,
                    AdditionalInfo: logAttribute.AdditionalInfo
                );

                await _userLogRepository.AddLog(userLog);
                _logger.LogInformation($"User action logged: {logAttribute.Action} by user {currentUser.Email}" +
                    (targetUser != null ? $" targeting {targetUser.Email}" : ""));
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Failed to log user action: {logAttribute.Action}");
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
        return result switch
        {
            OkObjectResult or OkResult or CreatedResult or CreatedAtActionResult or NoContentResult => true,
            ObjectResult objectResult => objectResult.StatusCode.HasValue &&
                                        objectResult.StatusCode >= 200 &&
                                        objectResult.StatusCode < 300,
            _ => false
        };
    }
}
