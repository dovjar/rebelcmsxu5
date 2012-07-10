using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.PublicAccess, "Public Access", FixedPermissionTypes.EntityAction, UserType.User)]
    public class PublicAccessPermission : Permission
    { }
}