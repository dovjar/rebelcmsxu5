using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Update, "Update", FixedPermissionTypes.EntityAction, UserType.User)]
    public class UpdatePermission : Permission
    { }
}