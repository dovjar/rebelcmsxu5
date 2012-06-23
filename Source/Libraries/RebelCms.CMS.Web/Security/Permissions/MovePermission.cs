using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Move, "Move", FixedPermissionTypes.EntityAction)]
    public class MovePermission : Permission
    { }
}