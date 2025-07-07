using Volo.Abp.Application.Services;
using UserNotifySignalRDemo.Localization;

namespace UserNotifySignalRDemo.Services;

/* Inherit your application services from this class. */
public abstract class UserNotifySignalRDemoAppService : ApplicationService
{
    protected UserNotifySignalRDemoAppService()
    {
        LocalizationResource = typeof(UserNotifySignalRDemoResource);
    }
}