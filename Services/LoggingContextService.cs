using RecamSystemApi.Models;

namespace RecamSystemApi.Services;

/// <summary>
/// Service to capture and store logging context during action execution.
/// This allows filters to collect data before and after action execution.
/// </summary>
public class LoggingContextService
{
    private ListingCase? _beforeListingCase;
    private ListingCase? _afterListingCase;
    private User? _targetUser;
    private User? _currentUser;
    private List<FieldChange>? _fieldChanges;

    /// <summary>
    /// Stores the state of a listing case before an operation
    /// </summary>
    public void SetBeforeListingCase(ListingCase listingCase)
    {
        _beforeListingCase = listingCase;
    }

    /// <summary>
    /// Stores the state of a listing case after an operation
    /// </summary>
    public void SetAfterListingCase(ListingCase listingCase)
    {
        _afterListingCase = listingCase;
    }

    /// <summary>
    /// Gets the listing case state before operation
    /// </summary>
    public ListingCase? GetBeforeListingCase() => _beforeListingCase;

    /// <summary>
    /// Gets the listing case state after operation
    /// </summary>
    public ListingCase? GetAfterListingCase() => _afterListingCase;

    /// <summary>
    /// Stores the target user for user-related operations
    /// </summary>
    public void SetTargetUser(User user)
    {
        _targetUser = user;
    }

    /// <summary>
    /// Gets the target user
    /// </summary>
    public User? GetTargetUser() => _targetUser;

    /// <summary>
    /// Stores the current user performing the action
    /// </summary>
    public void SetCurrentUser(User user)
    {
        _currentUser = user;
    }

    /// <summary>
    /// Gets the current user
    /// </summary>
    public User? GetCurrentUser() => _currentUser;

    /// <summary>
    /// Stores field changes for update operations
    /// </summary>
    public void SetFieldChanges(List<FieldChange> changes)
    {
        _fieldChanges = changes;
    }

    /// <summary>
    /// Gets field changes
    /// </summary>
    public List<FieldChange>? GetFieldChanges() => _fieldChanges;

    /// <summary>
    /// Clears all stored context (should be called after logging is complete)
    /// </summary>
    public void Clear()
    {
        _beforeListingCase = null;
        _afterListingCase = null;
        _targetUser = null;
        _currentUser = null;
        _fieldChanges = null;
    }
}
