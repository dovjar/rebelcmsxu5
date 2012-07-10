using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Save, "Save", FixedPermissionTypes.EntityAction, UserType.User)]
    public class SavePermission : Permission
    { }
}