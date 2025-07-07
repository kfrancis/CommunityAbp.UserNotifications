using UserNotifySignalRDemo.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace UserNotifySignalRDemo.Permissions;

public class UserNotifySignalRDemoPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(UserNotifySignalRDemoPermissions.GroupName);


        
        //Define your own permissions here. Example:
        //myGroup.AddPermission(UserNotifySignalRDemoPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<UserNotifySignalRDemoResource>(name);
    }
}
