using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.View, "View", FixedPermissionTypes.EntityAction)]
    public class ViewPermission : Permission
    { }
}