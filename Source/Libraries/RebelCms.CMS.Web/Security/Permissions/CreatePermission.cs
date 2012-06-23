using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Create, "Create", FixedPermissionTypes.EntityAction)]
    public class CreatePermission : Permission
    { }
}