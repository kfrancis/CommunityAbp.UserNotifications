using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace CommunityAbp.UserNotifications.Sse.Pages.Components.SseScripts;

public class SseScriptsViewComponent : AbpViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View("~/Pages/Components/SseScripts/Default.cshtml");
    }
}
