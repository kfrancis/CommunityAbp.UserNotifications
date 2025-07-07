using System.Text;
using System.Text.Json;
using CommunityAbp.UserNotifications.Abstractions;
using CommunityAbp.UserNotifications.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace CommunityAbp.UserNotifications.Sse.Services;

/// <summary>
///     Service for sending notifications to users, groups, or all connected clients via Server-Sent Events (SSE).
/// </summary>
public class SseNotificationSender : INotificationSender, ITransientDependency
{
    private readonly ISseConnectionManager _connectionManager;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<SseNotificationSender> _logger;
    private readonly UserNotificationsOptions _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SseNotificationSender" /> class.
    /// </summary>
    /// <param name="connectionManager">
    ///     The connection manager responsible for handling SSE connections.
    /// </param>
    /// <param name="options">
    ///     Configuration options for user notifications, including CORS settings and heartbeat intervals.
    /// </param>
    /// <param name="logger">
    ///     Logger for logging errors and information during notification sending operations.
    /// </param>
    public SseNotificationSender(
        ISseConnectionManager connectionManager,
        IOptions<UserNotificationsOptions> options,
        ILogger<SseNotificationSender> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
        _options = options.Value;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <summary>
    ///     Sends a notification to all connected clients with the specified event name and data.
    /// </summary>
    /// <param name="eventName">
    ///     The name of the event to send. This will be used as the event type in the SSE message.
    /// </param>
    /// <param name="data">
    ///     The data to send with the event. This will be serialized to JSON and sent as the event data.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation of sending the notification to all connected clients.
    /// </returns>
    public async Task SendToAllAsync(string eventName, object data)
    {
        foreach (var connectionId in _connectionManager.GetAllConnections())
            try
            {
                await SendToConnectionAsync(connectionId, eventName, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending to connection {ConnectionId}", connectionId);
            }
    }

    /// <summary>
    ///     Sends a notification to all connections associated with a specific user.
    /// </summary>
    /// <param name="userId">
    ///     The unique identifier of the user to send the notification to.
    /// </param>
    /// <param name="eventName">
    ///     The name of the event to send. This will be used as the event type in the SSE message.
    /// </param>
    /// <param name="data">
    ///     The data to send with the event. This will be serialized to JSON and sent as the event data.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation of sending the notification to all connections of the specified
    ///     user.
    /// </returns>
    public async Task SendToUserAsync(string userId, string eventName, object data)
    {
        foreach (var connectionId in _connectionManager.GetUserConnections(userId))
            try
            {
                await SendToConnectionAsync(connectionId, eventName, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending to user {UserId} connection {ConnectionId}", userId, connectionId);
            }
    }

    /// <summary>
    ///     Sends a notification to all connections in a specific group.
    /// </summary>
    /// <param name="groupName">
    ///     The name of the group to send the notification to. This should match the group name used when adding connections.
    /// </param>
    /// <param name="eventName">
    ///     The name of the event to send. This will be used as the event type in the SSE message.
    /// </param>
    /// <param name="data">
    ///     The data to send with the event. This will be serialized to JSON and sent as the event data.
    /// </param>
    public async Task SendToGroupAsync(string groupName, string eventName, object data)
    {
        foreach (var connectionId in _connectionManager.GetGroupConnections(groupName))
            try
            {
                await SendToConnectionAsync(connectionId, eventName, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending to group {GroupName} connection {ConnectionId}", groupName,
                    connectionId);
            }
    }

    /// <summary>
    ///     Sends a notification to a specific connection identified by its connection ID.
    /// </summary>
    /// <param name="connectionId">
    ///     The unique identifier of the connection to send the notification to.
    /// </param>
    /// <param name="eventName">
    ///     The name of the event to send. This will be used as the event type in the SSE message.
    /// </param>
    /// <param name="data">
    ///     The data to send with the event. This will be serialized to JSON and sent as the event data.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation of sending the notification to the specified connection.
    /// </returns>
    public async Task SendToConnectionAsync(Guid connectionId, string eventName, object data)
    {
        var connection = _connectionManager.GetConnection(connectionId);
        if (connection == null)
            return;

        try
        {
            var jsonData = JsonSerializer.Serialize(data, _jsonOptions);
            var message = $"event: {eventName}\ndata: {jsonData}\n\n";
            var bytes = Encoding.UTF8.GetBytes(message);

            var response = connection.Response;
            connection.LastActivityAt = DateTime.UtcNow;

            await response.Body.WriteAsync(bytes, connection.CancellationTokenSource.Token);
            await response.Body.FlushAsync(connection.CancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending to connection {ConnectionId}", connectionId);
            _connectionManager.RemoveConnection(connectionId);
        }
    }

    /// <summary>
    ///     Sends a notification message to all connected clients.
    /// </summary>
    /// <param name="message">
    ///     The message text to send to all clients. This will be included in the notification data.
    /// </param>
    /// <param name="severity">
    ///     The severity level of the notification. This can be used to indicate the importance or urgency of the message.
    /// </param>
    /// <param name="title">
    ///     An optional title for the notification. If provided, it will be included in the notification data.
    /// </param>
    public async Task NotifyAllAsync(string message, NotificationSeverity severity = NotificationSeverity.Info,
        string? title = null)
    {
        await SendToAllAsync("notification", new NotificationData
        {
            Message = message,
            Severity = severity,
            Title = title
        });
    }

    /// <summary>
    ///     Sends a notification message to a specific user.
    /// </summary>
    /// <param name="userId">
    ///     The unique identifier of the user to send the notification to.
    /// </param>
    /// <param name="message">
    ///     The message text to send to the user. This will be included in the notification data.
    /// </param>
    /// <param name="severity">
    ///     The severity level of the notification. This can be used to indicate the importance or urgency of the message.
    /// </param>
    /// <param name="title">
    ///     An optional title for the notification. If provided, it will be included in the notification data.
    /// </param>
    public async Task NotifyUserAsync(string userId, string message,
        NotificationSeverity severity = NotificationSeverity.Info, string? title = null)
    {
        await SendToUserAsync(userId, "notification", new NotificationData
        {
            Message = message,
            Severity = severity,
            Title = title
        });
    }

    /// <summary>
    ///     Sends a notification message to a specific group of users.
    /// </summary>
    /// <param name="groupName">
    ///     The name of the group to send the notification to.
    /// </param>
    /// <param name="message">
    ///     The message text to send to the group. This will be included in the notification data.
    /// </param>
    /// <param name="severity">
    ///     The severity level of the notification. This can be used to indicate the importance or urgency of the message.
    /// </param>
    /// <param name="title">
    ///     An optional title for the notification. If provided, it will be included in the notification data.
    /// </param>
    public async Task NotifyGroupAsync(string groupName, string message,
        NotificationSeverity severity = NotificationSeverity.Info, string? title = null)
    {
        await SendToGroupAsync(groupName, "notification", new NotificationData
        {
            Message = message,
            Severity = severity,
            Title = title
        });
    }
}