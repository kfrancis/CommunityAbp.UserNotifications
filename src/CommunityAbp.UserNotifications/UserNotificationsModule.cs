using CommunityAbp.UserNotifications.Abstractions;
using CommunityAbp.UserNotifications.Configuration;
using CommunityAbp.UserNotifications.Services;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Autofac;
using Volo.Abp.Caching;
using Volo.Abp.Modularity;

namespace CommunityAbp.UserNotifications;

/// <summary>
/// Module for managing user notifications, including sending notifications to users, groups, or all connected clients. 
/// </summary>
[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpCachingModule)
)]
public class UserNotificationsModule : AbpModule
{
    /// <summary>
    /// Configures the services for the UserNotifications module. 
    /// </summary>
    /// <param name="context">
    /// The service configuration context that provides access to the service collection and other configuration options.
    /// </param>
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<INotificationManager, NotificationManager>();

        // Configure options
        context.Services.Configure<UserNotificationsOptions>(options =>
        {
            options.HeartbeatInterval = 30; // seconds
        });

        // Configure controllers
        context.Services.AddControllers();
    }
}