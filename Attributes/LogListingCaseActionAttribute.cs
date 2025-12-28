namespace RecamSystemApi.Attributes;

/// <summary>
/// Attribute to mark controller actions that should log listing case changes to MongoDB.
/// When applied, the action will automatically create a ListingCaseLog entry.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class LogListingCaseActionAttribute : Attribute
{
    /// <summary>
    /// The type of change being performed
    /// </summary>
    public ChangeType ChangeType { get; }

    /// <summary>
    /// Additional information to include in the log (optional)
    /// </summary>
    public string? AdditionalInfo { get; set; }

    /// <summary>
    /// Whether to capture field changes (for Update operations)
    /// </summary>
    public bool CaptureFieldChanges { get; set; }

    /// <summary>
    /// Name of the route parameter that contains the listing case ID
    /// Default is "id" or "listingCaseId"
    /// </summary>
    public string? ListingCaseIdParameter { get; set; }

    public LogListingCaseActionAttribute(ChangeType changeType)
    {
        ChangeType = changeType;
        CaptureFieldChanges = changeType == ChangeType.Updated;
    }
}
