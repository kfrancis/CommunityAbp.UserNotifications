using Volo.Abp.Application.Services;
using UserNotifySseDemo.Localization;

namespace UserNotifySseDemo.Services;

/* Inherit your application services from this class. */
public abstract class UserNotifySseDemoAppService : ApplicationService
{
    protected UserNotifySseDemoAppService()
    {
        LocalizationResource = typeof(UserNotifySseDemoResource);
    }
}