using Microsoft.AspNetCore.Http;

namespace CommunityAbp.UserNotifications.Sse.Services;

/// <summary>
///   Interface for managing Server-Sent Events (SSE) connections.
/// </summary>
public interface ISseConnectionManager
{
    /// <summary>
    ///     Adds a new SSE connection.
    /// </summary>
    /// <param name="response">The HTTP response to keep open</param>
    /// <param name="userId">Optional user ID if the connection is authenticated</param>
    /// <param name="groups">Optional groups this connection should be part of</param>
    /// <returns>Connection ID</returns>
    Guid AddConnection(HttpResponse response, string? userId = null, IEnumerable<string>? groups = null);

    /// <summary>
    ///     Removes a connection by its ID.
    /// </summary>
    void RemoveConnection(Guid connectionId);

    /// <summary>
    ///     Gets a connection by its ID.
    /// </summary>
    ConnectionInfo? GetConnection(Guid connectionId);

    /// <summary>
    ///     Gets all connection IDs.
    /// </summary>
    IEnumerable<Guid> GetAllConnections();

    /// <summary>
    ///     Gets all connection IDs for a specific user.
    /// </summary>
    IEnumerable<Guid> GetUserConnections(string userId);

    /// <summary>
    ///     Gets all connection IDs for a specific group.
    /// </summary>
    IEnumerable<Guid> GetGroupConnections(string groupName);

    /// <summary>
    ///     Adds an existing connection to a group.
    /// </summary>
    void AddConnectionToGroup(Guid connectionId, string groupName);

    /// <summary>
    ///     Removes a connection from a group.
    /// </summary>
    void RemoveConnectionFromGroup(Guid connectionId, string groupName);
}

/// <summary>
///   Represents information about a single SSE connection.
/// </summary>
public class ConnectionInfo
{
    /// <summary>
    ///     Unique identifier for the connection.
    /// </summary>
    public required Guid ConnectionId { get; set; }

    /// <summary>
    ///     The HTTP response associated with this connection.
    /// </summary>
    public required HttpResponse Response { get; init; }

    /// <summary>
    ///     The ID of the user associated with this connection.
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    ///     A set of groups this connection belongs to.
    /// </summary>
    public HashSet<string> Groups { get; set; } = [];

    /// <summary>
    ///     Cancellation token source for managing the connection's lifetime.
    /// </summary>
    public CancellationTokenSource CancellationTokenSource { get; } = new();

    /// <summary>
    ///     The time when the connection was established.
    /// </summary>
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     The last time the connection was active.
    /// </summary>
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
}