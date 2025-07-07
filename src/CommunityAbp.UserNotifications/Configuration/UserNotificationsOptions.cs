namespace CommunityAbp.UserNotifications.Configuration;

/// <summary>
///     Configuration options for user notifications, including settings for Server-Sent Events (SSE) connections.
/// </summary>
public class UserNotificationsOptions
{
    /// <summary>
    ///     Interval in seconds to send heartbeat messages to keep the connection alive.
    ///     Default is 30 seconds.
    /// </summary>
    public int HeartbeatInterval { get; set; } = 30;

    /// <summary>
    ///     Default group name for notifications that don't specify a group.
    /// </summary>
    public string DefaultGroup { get; set; } = "General";

    /// <summary>
    ///     Enable cross-origin resource sharing for SSE connections.
    /// </summary>
    public bool EnableCors { get; set; } = false;

    /// <summary>
    ///     The allowed origins for CORS if enabled.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = ["*"];
}
