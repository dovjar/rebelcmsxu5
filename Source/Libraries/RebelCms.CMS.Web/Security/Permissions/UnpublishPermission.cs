using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Unpublish, "Unpublish", FixedPermissionTypes.EntityAction)]
    public class UnpublishPermission : Permission
    { }
}