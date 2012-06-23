using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Delete, "Delete", FixedPermissionTypes.EntityAction)]
    public class DeletePermission : Permission
    { }
}