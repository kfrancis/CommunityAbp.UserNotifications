using CommunityAbp.UserNotifications.Abstractions;
using CommunityAbp.UserNotifications.Sse.Services;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;
using Volo.Abp.Ui.LayoutHooks;
using Volo.Abp.VirtualFileSystem;

namespace CommunityAbp.UserNotifications.Sse
{
    /// <summary>
    /// Module for managing user notifications via Server-Sent Events (SSE), allowing real-time communication of notifications to connected clients. 
    /// </summary>
    [DependsOn(
        typeof(UserNotificationsModule),
        typeof(AbpAspNetCoreMvcModule)
    )]
    public class SseUserNotificationsModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            // Register SSE services
            context.Services.AddSingleton<ISseConnectionManager, SseConnectionManager>();
            context.Services.AddTransient<INotificationSender, SseNotificationSender>();

            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<SseUserNotificationsModule>();
            });

            // Configure bundling
            Configure<AbpLayoutHookOptions>(options => {
                options.Add(
                    LayoutHooks.Head.Last,
                    typeof(SseUserNotificationsModule),
                    "_SseScriptsPartial"
                );
            });
        }
    }
}
