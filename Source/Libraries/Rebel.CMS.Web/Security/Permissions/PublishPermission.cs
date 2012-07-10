using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Publish, "Publish", FixedPermissionTypes.EntityAction, UserType.User)]
    public class PublishPermission : Permission
    { }
}