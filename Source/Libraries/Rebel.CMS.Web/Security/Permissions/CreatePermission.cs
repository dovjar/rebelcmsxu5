using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Create, "Create", FixedPermissionTypes.EntityAction, UserType.User)]
    public class CreatePermission : Permission
    { }
}