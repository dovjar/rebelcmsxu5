using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Update, "Update", FixedPermissionTypes.EntityAction)]
    public class UpdatePermission : Permission
    { }
}