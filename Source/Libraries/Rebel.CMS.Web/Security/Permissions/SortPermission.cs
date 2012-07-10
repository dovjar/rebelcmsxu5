using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Sort, "Sort", FixedPermissionTypes.EntityAction, UserType.User)]
    public class SortPermission : Permission
    { }
}