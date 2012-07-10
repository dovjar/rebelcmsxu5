using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.View, "View", FixedPermissionTypes.EntityAction, UserType.User)]
    public class ViewPermission : Permission
    { }
}