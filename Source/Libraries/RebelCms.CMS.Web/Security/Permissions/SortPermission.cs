using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Sort, "Sort", FixedPermissionTypes.EntityAction)]
    public class SortPermission : Permission
    { }
}