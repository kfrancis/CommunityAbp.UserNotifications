using CommunityAbp.UserNotifications.Abstractions;
using CommunityAbp.UserNotifications.Configuration;
using CommunityAbp.UserNotifications.Services;
using CommunityAbp.UserNotifications.Sse.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Volo.Abp.AspNetCore.Mvc;

namespace CommunityAbp.UserNotifications.Sse.Controllers;

/// <summary>
///     Controller for managing Server-Sent Events (SSE) connections and groups.
/// </summary>
[Route("api/notifications/sse")]
public class SseController : AbpController
{
    private readonly ISseConnectionManager _connectionManager;
    private readonly INotificationSender _notificationSender;
    private readonly UserNotificationsOptions _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SseController" /> class.
    /// </summary>
    /// <param name="connectionManager">
    ///     The connection manager responsible for handling SSE connections.
    /// </param>
    /// <param name="notificationSender">
    ///     The service for sending notifications to SSE connections.
    /// </param>
    /// <param name="options">
    ///     Configuration options for user notifications, including CORS settings and heartbeat intervals.
    /// </param>
    public SseController(
        ISseConnectionManager connectionManager,
        INotificationSender notificationSender,
        IOptions<UserNotificationsOptions> options)
    {
        _connectionManager = connectionManager;
        _notificationSender = notificationSender;
        _options = options.Value;
    }

    /// <summary>
    ///     Establishes a Server-Sent Events (SSE) connection.
    /// </summary>
    /// <param name="groupName">
    ///     Optional name of the group to join. If not specified, the default group will be used.
    /// </param>
    [HttpGet("connect")]
    public async Task Connect(string? groupName = null)
    {
        var response = Response;
        var request = Request;

        // Set SSE headers
        response.Headers.Append("Content-Type", "text/event-stream");
        response.Headers.Append("Cache-Control", "no-cache");
        response.Headers.Append("Connection", "keep-alive");

        // Set CORS headers if enabled
        if (_options.EnableCors)
        {
            var origin = request.Headers["Origin"].FirstOrDefault();
            if (!string.IsNullOrEmpty(origin) &&
                (_options.AllowedOrigins.Contains("*") || _options.AllowedOrigins.Contains(origin)))
            {
                response.Headers.Append("Access-Control-Allow-Origin", origin);
                response.Headers.Append("Access-Control-Allow-Credentials", "true");
            }
        }

        // Get user ID if authenticated
        string? userId = null;
        if (CurrentUser.IsAuthenticated) userId = CurrentUser.Id.ToString();

        // Add the connection
        var groups = groupName != null
            ? new[] { groupName }
            : new[] { _options.DefaultGroup };

        var connectionId = _connectionManager.AddConnection(response, userId, groups);

        // Send initial connection established event
        await _notificationSender.SendToConnectionAsync(connectionId, "connected", new
        {
            ConnectionId = connectionId,
            Groups = groups,
            UserId = userId,
            Timestamp = DateTime.UtcNow
        });

        // Setup heartbeat timer
        var timer = new Timer(async void (_) =>
        {
            try
            {
                await _notificationSender.SendToConnectionAsync(connectionId, "heartbeat", new
                {
                    Timestamp = DateTime.UtcNow
                });
            }
            catch
            {
                // Connection likely closed
            }
        }, null, TimeSpan.FromSeconds(_options.HeartbeatInterval), TimeSpan.FromSeconds(_options.HeartbeatInterval));

        // Keep the connection open until client disconnects
        var tcs = new TaskCompletionSource<bool>();
        HttpContext.RequestAborted.Register(() =>
        {
            _connectionManager.RemoveConnection(connectionId);
            timer.Dispose();
            tcs.TrySetResult(true);
        });

        await tcs.Task;
    }

    /// <summary>
    ///     Joins a group for the specified connection.
    /// </summary>
    /// <param name="connectionId">
    ///     The unique identifier of the connection to join the group.
    /// </param>
    /// <param name="groupName">
    ///     The name of the group to join. If empty, returns BadRequest.
    /// </param>
    /// <returns>
    ///     Returns Ok if the connection was successfully added to the group, or NotFound if the connection does not exist.
    /// </returns>
    [HttpPost("group/join")]
    public IActionResult JoinGroup(Guid connectionId, string groupName)
    {
        if (string.IsNullOrEmpty(groupName)) return BadRequest("Group name cannot be empty");

        var connection = _connectionManager.GetConnection(connectionId);
        if (connection == null) return NotFound("Connection not found");

        _connectionManager.AddConnectionToGroup(connectionId, groupName);
        return Ok();
    }

    /// <summary>
    ///     Leaves a group for the specified connection.
    /// </summary>
    /// <param name="connectionId">
    ///     The unique identifier of the connection to leave the group.
    /// </param>
    /// <param name="groupName">
    ///     The name of the group to leave. If empty, returns BadRequest.
    /// </param>
    /// <returns>
    ///     Returns Ok if the connection was successfully removed from the group, or NotFound if the connection does not exist.
    /// </returns>
    [HttpPost("group/leave")]
    public IActionResult LeaveGroup(Guid connectionId, string groupName)
    {
        if (string.IsNullOrEmpty(groupName)) return BadRequest("Group name cannot be empty");

        var connection = _connectionManager.GetConnection(connectionId);
        if (connection == null) return NotFound("Connection not found");

        _connectionManager.RemoveConnectionFromGroup(connectionId, groupName);
        return Ok();
    }
}