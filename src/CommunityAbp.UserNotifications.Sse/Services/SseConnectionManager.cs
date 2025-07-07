using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;

namespace CommunityAbp.UserNotifications.Sse.Services;

/// <summary>
///     Manages Server-Sent Events (SSE) connections.
/// </summary>
public class SseConnectionManager : ISseConnectionManager
{
    private readonly ConcurrentDictionary<Guid, ConnectionInfo> _connections = new();
    private readonly ConcurrentDictionary<string, HashSet<Guid>> _groupConnections = new();
    private readonly ConcurrentDictionary<string, HashSet<Guid>> _userConnections = new();

    /// <summary>
    ///     Adds a new SSE connection.
    /// </summary>
    /// <param name="response">
    ///     The HTTP response to write the SSE data to.
    /// </param>
    /// <param name="userId">
    ///     The ID of the user associated with this connection. If null, the connection is anonymous.
    /// </param>
    /// <param name="groups">
    ///     Optional list of groups this connection belongs to. If specified, the connection will be added to these groups.
    /// </param>
    /// <returns>
    ///     The unique identifier for the newly created connection.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when the userId is null or empty.
    /// </exception>
    public Guid AddConnection(HttpResponse response, string? userId = null, IEnumerable<string>? groups = null)
    {
        var connectionId = Guid.NewGuid();
        var connectionInfo = new ConnectionInfo
        {
            ConnectionId = connectionId,
            Response = response,
            UserId = userId ?? throw new ArgumentNullException(nameof(userId))
        };

        // Add to groups if specified
        if (groups != null)
            foreach (var group in groups)
            {
                connectionInfo.Groups.Add(group);
                AddToGroupInternal(connectionId, group);
            }

        // Track connection by user if authenticated
        if (!string.IsNullOrEmpty(userId))
        {
            connectionInfo.UserId = userId;
            _userConnections.AddOrUpdate(
                userId,
                [connectionId],
                (_, existingSet) =>
                {
                    lock (existingSet)
                    {
                        existingSet.Add(connectionId);
                        return existingSet;
                    }
                });
        }

        // Store the connection
        _connections.TryAdd(connectionId, connectionInfo);

        return connectionId;
    }

    /// <summary>
    ///     Removes a connection by its unique identifier.
    /// </summary>
    /// <param name="connectionId">
    ///     The unique identifier of the connection to remove.
    /// </param>
    public void RemoveConnection(Guid connectionId)
    {
        if (!_connections.TryRemove(connectionId, out var connection)) return;

        // Cancel any pending operations
        connection.CancellationTokenSource.Cancel();

        // Remove from user connections
        if (!string.IsNullOrEmpty(connection.UserId) &&
            _userConnections.TryGetValue(connection.UserId, out var userConnections))
            lock (userConnections)
            {
                userConnections.Remove(connectionId);
                if (userConnections.Count == 0) _userConnections.TryRemove(connection.UserId, out _);
            }

        // Remove from all groups
        foreach (var group in connection.Groups)
            if (_groupConnections.TryGetValue(group, out var groupConnections))
                lock (groupConnections)
                {
                    groupConnections.Remove(connectionId);
                    if (groupConnections.Count == 0) _groupConnections.TryRemove(group, out _);
                }
    }

    /// <summary>
    ///     Retrieves a connection by its unique identifier.
    /// </summary>
    /// <param name="connectionId">
    ///     The unique identifier of the connection to retrieve.
    /// </param>
    /// <returns>
    ///     The <see cref="ConnectionInfo" /> object representing the connection.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    ///     Thrown when no connection with the specified ID exists.
    /// </exception>
    public ConnectionInfo? GetConnection(Guid connectionId)
    {
        return _connections.TryGetValue(connectionId, out var connection) ? connection : null;
    }

    /// <summary>
    ///     Retrieves all active connections.
    /// </summary>
    /// <returns>
    ///     An enumerable collection of unique identifiers for all active connections.
    /// </returns>
    public IEnumerable<Guid> GetAllConnections()
    {
        return _connections.Keys;
    }

    /// <summary>
    ///     Retrieves all connections associated with a specific user.
    /// </summary>
    /// <param name="userId">
    ///     The ID of the user whose connections to retrieve.
    /// </param>
    /// <returns>
    ///     An enumerable collection of unique identifiers for the user's connections.
    /// </returns>
    public IEnumerable<Guid> GetUserConnections(string userId)
    {
        if (string.IsNullOrEmpty(userId) || !_userConnections.TryGetValue(userId, out var connections)) return [];

        lock (connections)
        {
            return connections.ToList();
        }
    }

    /// <summary>
    ///     Retrieves all connections associated with a specific group.
    /// </summary>
    /// <param name="groupName">
    ///     The name of the group whose connections to retrieve.
    /// </param>
    /// <returns>
    ///     An enumerable collection of unique identifiers for the group's connections.
    /// </returns>
    public IEnumerable<Guid> GetGroupConnections(string groupName)
    {
        if (string.IsNullOrEmpty(groupName) ||
            !_groupConnections.TryGetValue(groupName, out var connections)) return [];

        lock (connections)
        {
            return connections.ToList();
        }
    }

    /// <summary>
    ///     Adds an existing connection to a specified group.
    /// </summary>
    /// <param name="connectionId">
    ///     The unique identifier of the connection to add.
    /// </param>
    /// <param name="groupName">
    ///     The name of the group to which the connection should be added.
    /// </param>
    public void AddConnectionToGroup(Guid connectionId, string groupName)
    {
        if (string.IsNullOrEmpty(groupName))
            return;

        if (!_connections.TryGetValue(connectionId, out var connection)) return;
        connection.Groups.Add(groupName);

        AddToGroupInternal(connectionId, groupName);
    }

    /// <summary>
    ///     Removes a connection from a specified group.
    /// </summary>
    /// <param name="connectionId">
    ///     The unique identifier of the connection to remove.
    /// </param>
    /// <param name="groupName">
    ///     The name of the group from which the connection should be removed.
    /// </param>
    public void RemoveConnectionFromGroup(Guid connectionId, string groupName)
    {
        if (string.IsNullOrEmpty(groupName))
            return;

        if (!_connections.TryGetValue(connectionId, out var connection)) return;
        connection.Groups.Remove(groupName);

        if (!_groupConnections.TryGetValue(groupName, out var groupConnections)) return;
        lock (groupConnections)
        {
            groupConnections.Remove(connectionId);
            if (groupConnections.Count == 0) _groupConnections.TryRemove(groupName, out _);
        }
    }

    /// <summary>
    ///     Internal method to add a connection to a group.
    /// </summary>
    /// <param name="connectionId">
    ///     The unique identifier of the connection to add.
    /// </param>
    /// <param name="groupName">
    ///     The name of the group to which the connection should be added.
    /// </param>
    private void AddToGroupInternal(Guid connectionId, string groupName)
    {
        _groupConnections.AddOrUpdate(
            groupName,
            [connectionId],
            (_, existingSet) =>
            {
                lock (existingSet)
                {
                    existingSet.Add(connectionId);
                    return existingSet;
                }
            });
    }
}