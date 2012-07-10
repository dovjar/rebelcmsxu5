using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Copy, "Copy", FixedPermissionTypes.EntityAction, UserType.User)]
    public class CopyPermission : Permission
    { }
}