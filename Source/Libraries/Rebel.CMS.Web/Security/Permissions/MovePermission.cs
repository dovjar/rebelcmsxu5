using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Move, "Move", FixedPermissionTypes.EntityAction, UserType.User)]
    public class MovePermission : Permission
    { }
}