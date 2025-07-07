using UserNotifySseDemo.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace UserNotifySseDemo.Permissions;

public class UserNotifySseDemoPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(UserNotifySseDemoPermissions.GroupName);


        
        //Define your own permissions here. Example:
        //myGroup.AddPermission(UserNotifySseDemoPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<UserNotifySseDemoResource>(name);
    }
}
