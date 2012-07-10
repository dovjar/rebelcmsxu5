using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.BackOfficeAccess, "Back-Office Access", FixedPermissionTypes.System, UserType.User)]
    public class BackOfficeAccessPermission : Permission
    { }
}