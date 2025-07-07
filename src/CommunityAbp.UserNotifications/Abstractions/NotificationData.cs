namespace CommunityAbp.UserNotifications.Abstractions;

/// <summary>
///     Represents the data structure for a notification.
/// </summary>
public class NotificationData
{
    /// <summary>
    ///     The message content of the notification.
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    ///     The severity level of the notification, indicating its importance or urgency.
    /// </summary>
    public NotificationSeverity Severity { get; set; } = NotificationSeverity.Info;

    /// <summary>
    ///     An optional title for the notification.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    ///     Additional properties that can be included in the notification.
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = [];
}