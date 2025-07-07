namespace CommunityAbp.UserNotifications.Abstractions;

/// <summary>
///     Defines a transport mechanism for sending notifications
/// </summary>
public interface INotificationSender
{
    /// <summary>
    ///     Sends a notification to all connected clients with the specified event name and data.
    /// </summary>
    /// <param name="eventName">
    ///     The name of the event to send. This will be used by the client to handle the event appropriately.
    /// </param>
    /// <param name="data">
    ///     The data to send with the event. This can be any serializable object that the client can process.
    /// </param>
    Task SendToAllAsync(string eventName, object data);

    /// <summary>
    ///     Sends a notification to a specific user identified by their user ID.
    /// </summary>
    /// <param name="userId">
    ///     The unique identifier of the user to whom the notification should be sent.
    /// </param>
    /// <param name="eventName">
    ///     The name of the event to send. This will be used by the client to handle the event appropriately.
    /// </param>
    /// <param name="data">
    ///     The data to send with the event. This can be any serializable object that the client can process.
    /// </param>
    Task SendToUserAsync(string userId, string eventName, object data);

    /// <summary>
    ///     Sends a notification to a specific group of users identified by the group name.
    /// </summary>
    /// <param name="groupName">
    ///     The name of the group to which the notification should be sent.
    /// </param>
    /// <param name="eventName">
    ///     The name of the event to send. This will be used by the client to handle the event appropriately.
    /// </param>
    /// <param name="data">
    ///     The data to send with the event. This can be any serializable object that the client can process.
    /// </param>
    Task SendToGroupAsync(string groupName, string eventName, object data);

    /// <summary>
    ///     Sends a notification to a specific connection identified by its connection ID.
    /// </summary>
    /// <param name="connectionId">
    ///     The unique identifier of the connection to which the notification should be sent.
    /// </param>
    /// <param name="eventName">
    ///     The name of the event to send. This will be used by the client to handle the event appropriately.
    /// </param>
    /// <param name="data">
    ///     The data to send with the event. This can be any serializable object that the client can process.
    /// </param>
    Task SendToConnectionAsync(Guid connectionId, string eventName, object data);
}