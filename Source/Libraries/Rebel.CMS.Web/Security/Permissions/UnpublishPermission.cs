using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Unpublish, "Unpublish", FixedPermissionTypes.EntityAction, UserType.User)]
    public class UnpublishPermission : Permission
    { }
}