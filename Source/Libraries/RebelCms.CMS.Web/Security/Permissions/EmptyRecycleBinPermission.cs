using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.EmptyRecycleBin, "Empty Recycle Bin", FixedPermissionTypes.EntityAction)]
    public class EmptyRecycleBinPermission : Permission
    { }
}