using Microsoft.Extensions.Localization;
using UserNotifySignalRDemo.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace UserNotifySignalRDemo;

[Dependency(ReplaceServices = true)]
public class UserNotifySignalRDemoBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<UserNotifySignalRDemoResource> _localizer;

    public UserNotifySignalRDemoBrandingProvider(IStringLocalizer<UserNotifySignalRDemoResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}