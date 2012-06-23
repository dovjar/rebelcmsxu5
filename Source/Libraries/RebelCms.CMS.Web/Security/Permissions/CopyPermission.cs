using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Copy, "Copy", FixedPermissionTypes.EntityAction)]
    public class CopyPermission : Permission
    { }
}