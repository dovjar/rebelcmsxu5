using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Rollback, "Rollback", FixedPermissionTypes.EntityAction, UserType.User)]
    public class RollbackPermission : Permission
    { }
}