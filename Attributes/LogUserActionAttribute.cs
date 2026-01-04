namespace RecamSystemApi.Attributes;

/// <summary>
/// Attribute to mark controller actions that should log user activities to MongoDB.
/// When applied, the action will automatically create a UserLog entry.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class LogUserActionAttribute : Attribute
{
    /// <summary>
    /// The type of user action being performed
    /// </summary>
    public UserAction Action { get; }

    /// <summary>
    /// Additional information to include in the log (optional)
    /// </summary>
    public string? AdditionalInfo { get; set; }

    /// <summary>
    /// Whether to log the target user from the action result (default: false)
    /// Set to true for actions like CreateAgent, DeleteUser where the result contains the target user
    /// </summary>
    public bool LogTargetUser { get; set; }

    /// <summary>
    /// Name of the route parameter that contains the target user ID (optional)
    /// Example: "id" for actions like DeleteUser/{id}
    /// </summary>
    public string? TargetUserIdParameter { get; set; }

    public LogUserActionAttribute(UserAction action)
    {
        Action = action;
    }
}
