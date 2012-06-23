using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Rollback, "Rollback", FixedPermissionTypes.EntityAction)]
    public class RollbackPermission : Permission
    { }
}