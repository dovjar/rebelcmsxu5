using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Sort, "Sort", FixedPermissionTypes.EntityAction, UserType.User)]
    public class SortPermission : Permission
    { }
}