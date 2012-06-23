using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Permissions, "Permissions", FixedPermissionTypes.EntityAction)]
    public class PermissionsPermission : Permission
    { }
}