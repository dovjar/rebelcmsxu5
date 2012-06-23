using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.BackOfficeAccess, "Back-Office Access", FixedPermissionTypes.System)]
    public class BackOfficeAccessPermission : Permission
    { }
}