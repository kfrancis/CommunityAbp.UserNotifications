using Volo.Abp.Modularity;

namespace CommunityAbp.UserNotifications.SignalR;

/// <summary>
/// Module for managing user notifications via SignalR, allowing real-time communication of notifications to connected clients. 
/// </summary>
[DependsOn(
    typeof(UserNotificationsModule)
)]
public class SignalrUserNotificationsModule : AbpModule
{

}
