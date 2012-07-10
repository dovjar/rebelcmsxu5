using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Permissions, "Permissions", FixedPermissionTypes.EntityAction, UserType.User)]
    public class PermissionsPermission : Permission
    { }
}