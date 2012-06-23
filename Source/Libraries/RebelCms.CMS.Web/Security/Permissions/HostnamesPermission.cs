using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Hostnames, "Hostnames", FixedPermissionTypes.EntityAction)]
    public class HostnamesPermission : Permission
    { }
}