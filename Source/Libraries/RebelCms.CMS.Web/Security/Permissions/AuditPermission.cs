using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Audit, "Audit", FixedPermissionTypes.EntityAction)]
    public class AuditPermission : Permission
    { }
}
