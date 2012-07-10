using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Translate, "Translate", FixedPermissionTypes.EntityAction, UserType.User)]
    public class TranslatePermission : Permission
    { }
}