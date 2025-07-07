using Microsoft.Extensions.Localization;
using UserNotifySseDemo.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace UserNotifySseDemo;

[Dependency(ReplaceServices = true)]
public class UserNotifySseDemoBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<UserNotifySseDemoResource> _localizer;

    public UserNotifySseDemoBrandingProvider(IStringLocalizer<UserNotifySseDemoResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}