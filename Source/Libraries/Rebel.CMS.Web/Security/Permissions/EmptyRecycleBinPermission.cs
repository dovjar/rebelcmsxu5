using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.EmptyRecycleBin, "Empty Recycle Bin", FixedPermissionTypes.EntityAction, UserType.User)]
    public class EmptyRecycleBinPermission : Permission
    { }
}