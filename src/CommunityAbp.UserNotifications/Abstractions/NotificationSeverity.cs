namespace CommunityAbp.UserNotifications.Abstractions;

/// <summary>
///     Service for sending notifications to users, groups, or all connected clients via Server-Sent Events (SSE).
/// </summary>
public enum NotificationSeverity
{
    /// <summary>
    ///     Represents a notification with no specific severity.
    /// </summary>
    Success,

    /// <summary>
    ///     Represents an informational notification.
    /// </summary>
    Info,

    /// <summary>
    ///     Represents a notification that indicates a warning or caution.
    /// </summary>
    Warning,

    /// <summary>
    ///     Represents a notification that indicates an error or critical issue.
    /// </summary>
    Error
}