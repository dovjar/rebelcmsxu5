using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Audit, "Audit", FixedPermissionTypes.EntityAction, UserType.User)]
    public class AuditPermission : Permission
    { }
}
