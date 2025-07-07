namespace CommunityAbp.UserNotifications.Abstractions;

/// <summary>
///     Main entry point for sending notifications
/// </summary>
public interface INotificationManager
{
    /// <summary>
    ///     Sends a notification to all connected users with the specified message, severity, and optional title.
    /// </summary>
    /// <param name="message">
    ///     The message to send in the notification. This should be a user-friendly string that describes the notification.
    /// </param>
    /// <param name="severity">
    ///     The severity level of the notification, which can be used to indicate the importance or urgency of the message.
    /// </param>
    /// <param name="title">
    ///     An optional title for the notification. If provided, this will be displayed prominently in the notification UI.
    /// </param>
    Task NotifyAllAsync(string message, NotificationSeverity severity = NotificationSeverity.Info,
        string? title = null);

    /// <summary>
    ///     Sends a notification to a specific user identified by their user ID.
    /// </summary>
    /// <param name="userId">
    ///     The unique identifier of the user to whom the notification should be sent.
    /// </param>
    /// <param name="message">
    ///     The message to send in the notification. This should be a user-friendly string that describes the notification.
    /// </param>
    /// <param name="severity">
    ///     The severity level of the notification, which can be used to indicate the importance or urgency of the message.
    /// </param>
    /// <param name="title">
    ///     An optional title for the notification. If provided, this will be displayed prominently in the notification UI.
    /// </param>
    Task NotifyUserAsync(string userId, string message, NotificationSeverity severity = NotificationSeverity.Info,
        string? title = null);

    /// <summary>
    ///     Sends a notification to a specific group of users identified by the group name.
    /// </summary>
    /// <param name="groupName">
    ///     The name of the group to which the notification should be sent. This allows for targeted notifications to specific
    ///     user groups.
    /// </param>
    /// <param name="message">
    ///     The message to send in the notification. This should be a user-friendly string that describes the notification.
    /// </param>
    /// <param name="severity">
    ///     The severity level of the notification, which can be used to indicate the importance or urgency of the message.
    /// </param>
    /// <param name="title">
    ///     An optional title for the notification. If provided, this will be displayed prominently in the notification UI.
    /// </param>
    Task NotifyGroupAsync(string groupName, string message, NotificationSeverity severity = NotificationSeverity.Info,
        string? title = null);

    /// <summary>
    ///     Sends a custom event with data to all connected users, or optionally to a specific user or group.
    /// </summary>
    /// <param name="eventName">
    ///     The name of the event to send. This will be used by the client to handle the event appropriately.
    /// </param>
    /// <param name="data">
    ///     The data to send with the event. This can be any serializable object that the client can process.
    /// </param>
    /// <param name="userId">
    ///     Optional user ID to send the event to a specific user. If null, the event will be sent to all users.
    /// </param>
    /// <param name="groupName">
    ///     Optional group name to send the event to a specific group of users. If null, the event will be sent to all users.
    /// </param>
    Task SendEventAsync(string eventName, object data, string? userId = null, string? groupName = null);
}