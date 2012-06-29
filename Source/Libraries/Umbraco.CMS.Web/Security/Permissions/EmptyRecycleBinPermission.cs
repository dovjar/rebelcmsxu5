using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.EmptyRecycleBin, "Empty Recycle Bin", FixedPermissionTypes.EntityAction, UserType.User)]
    public class EmptyRecycleBinPermission : Permission
    { }
}