# CommunityAbp.UserNotifications

A modular real-time user notification system for [ABP.io](https://abp.io) applications that provides a flexible, transport-agnostic approach to sending notifications to clients.

## Overview

CommunityAbp.UserNotifications enables you to easily implement real-time notifications in your ABP.io applications. It uses a modular architecture that separates the core notification concepts from specific transport implementations, allowing you to choose the transport mechanism that best fits your needs.

Currently supported transport mechanisms:
- **Server-Sent Events (SSE)** - Lightweight, one-way communication from server to client
- **SignalR** - Full-duplex communication between server and client

## Background

I was reading [this article](https://medium.com/@denmaklucky/you-dont-need-signalr-for-real-time-updates-server-sent-events-in-net-c-e032ff5d096e) and wanted to see if 
we could make a notification system that is not tied to SignalR, but can use it if needed. We've already integrated SignalR for this notification purpose (like notifying a user when a report is ready), but I wanted to create a more modular and flexible system that allows for different transport mechanisms.

## Installation

### 1. Install the required packages

```bash
# Install the core package
dotnet add package CommunityAbp.UserNotifications

# Install the transport implementation(s) you need
dotnet add package CommunityAbp.UserNotifications.Sse
dotnet add package CommunityAbp.UserNotifications.SignalR
```

### 2. Add module dependencies to your application

```csharp
[DependsOn(
    // ... other dependencies
    
    // Core module is included automatically when adding a transport
    typeof(CommunityAbpUserNotificationsSseModule),      // If using SSE
    typeof(CommunityAbpUserNotificationsSignalRModule)   // If using SignalR
)]
public class YourAppModule : AbpModule
{
    // ...
}
```

## Architecture

The notification system is built on a layered architecture:

- **Core Layer** (`CommunityAbp.UserNotifications`) - Contains abstractions and the notification manager
- **Transport Layer** (`CommunityAbp.UserNotifications.Sse`, `CommunityAbp.UserNotifications.SignalR`) - Implements specific transport mechanisms

This modular approach allows you to:
- Use multiple transport mechanisms simultaneously
- Add custom transport mechanisms by implementing the `INotificationSender` interface
- Swap transport implementations without changing your application code

## Usage

### Basic Usage

```csharp
public class YourService
{
    private readonly INotificationManager _notificationManager;
    
    public YourService(INotificationManager notificationManager)
    {
        _notificationManager = notificationManager;
    }
    
    public async Task DoSomethingAndNotify()
    {
        // Your business logic...
        
        // Send notification to all connected clients
        await _notificationManager.NotifyAllAsync(
            "Operation completed successfully!", 
            NotificationSeverity.Success, 
            "Success"
        );
        
        // Or send to a specific user
        await _notificationManager.NotifyUserAsync(
            userId: "user123",
            message: "Your task was processed",
            severity: NotificationSeverity.Info,
            title: "Task Update"
        );
        
        // Or send to a group of users
        await _notificationManager.NotifyGroupAsync(
            groupName: "Admins",
            message: "New user registered",
            severity: NotificationSeverity.Info,
            title: "User Registration"
        );
    }
}
```

### Sending Custom Events

You can send custom event data beyond basic notifications:

```csharp
await _notificationManager.SendEventAsync(
    eventName: "task-completed", 
    data: new { 
        TaskId = 123, 
        Status = "Completed", 
        CompletedAt = DateTime.UtcNow 
    }
);
```

## Client-Side Integration

### SSE Client

```javascript
// Initialize the SSE client
const sseClient = abp.sseNotifications.createClient({
    onConnected: (connectionId, data) => {
        console.log('Connected with ID:', connectionId);
    },
    onDisconnected: () => {
        console.log('Disconnected');
    }
});

// Connect to the SSE endpoint
sseClient.connect().then(connectionId => {
    console.log('Connected with ID:', connectionId);
});

// Listen for notifications
sseClient.on('notification', (data) => {
    console.log('Notification received:', data);
    // Show notification using your UI framework
    // e.g., toastr.success(data.message, data.title);
});

// Listen for custom events
sseClient.on('task-completed', (data) => {
    console.log('Task completed:', data);
    // Update UI based on the task data
});

// Join a group
sseClient.joinGroup('Admins').then(() => {
    console.log('Joined Admins group');
});

// Leave a group
sseClient.leaveGroup('Admins').then(() => {
    console.log('Left Admins group');
});

// Disconnect when needed
sseClient.disconnect();
```

### SignalR Client

```javascript
// Initialize the SignalR connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/notifications')
    .withAutomaticReconnect()
    .build();

// Start the connection
connection.start().then(() => {
    console.log('Connected to SignalR hub');
});

// Listen for notifications
connection.on('notification', (data) => {
    console.log('Notification received:', data);
    // Show notification using your UI framework
});

// Listen for custom events
connection.on('task-completed', (data) => {
    console.log('Task completed:', data);
    // Update UI based on the task data
});

// Join a group
connection.invoke('JoinGroup', 'Admins').then(() => {
    console.log('Joined Admins group');
});

// Leave a group
connection.invoke('LeaveGroup', 'Admins').then(() => {
    console.log('Left Admins group');
});

// Stop the connection when needed
connection.stop();
```

## Configuration Options

### Core Configuration

Configure notification options in your module:

```csharp
Configure<UserNotificationsOptions>(options =>
{
    options.DefaultGroup = "General";
    options.EnabledTransports = new[] { "Sse", "SignalR" };
});
```

### SSE-Specific Configuration

```csharp
Configure<SseOptions>(options =>
{
    options.HeartbeatInterval = 30; // seconds
    options.EnableCors = true;
    options.AllowedOrigins = new[] { "https://yourdomain.com" };
});
```

### SignalR-Specific Configuration

```csharp
Configure<SignalROptions>(options =>
{
    options.HubPath = "/hubs/notifications";
    options.EnableDetailedErrors = true; // For development
});
```

## API Reference

### INotificationManager

The main interface for sending notifications:

- `NotifyAllAsync(string message, NotificationSeverity severity, string title)` - Send notification to all connected clients
- `NotifyUserAsync(string userId, string message, NotificationSeverity severity, string title)` - Send notification to a specific user
- `NotifyGroupAsync(string groupName, string message, NotificationSeverity severity, string title)` - Send notification to a group
- `SendEventAsync(string eventName, object data, string userId, string groupName)` - Send a custom event with arbitrary data

### INotificationSender

Interface for transport implementations:

- `SendToAllAsync(string eventName, object data)` - Send data to all clients
- `SendToUserAsync(string userId, string eventName, object data)` - Send data to a specific user
- `SendToGroupAsync(string groupName, string eventName, object data)` - Send data to a group

### NotificationSeverity

Enum for notification severity levels:

- `Success` - Positive action completed
- `Info` - Informational message
- `Warning` - Warning message
- `Error` - Error message

## Demo Application

A sample application demonstrating the usage of the notification system is available in the `samples` directory. It showcases:

- Basic notification sending
- Group management
- Multiple transport types
- Client-side integration
- Angular integration

To run the demo:

```bash
cd samples/UserNotificationsDemo
dotnet run
```

Then navigate to `https://localhost:44302/Notifications` to see the notifications in action.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Credits

Developed by the Community ABP contributors.
