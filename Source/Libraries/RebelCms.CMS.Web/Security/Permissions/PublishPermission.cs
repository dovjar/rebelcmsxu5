using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Publish, "Publish", FixedPermissionTypes.EntityAction)]
    public class PublishPermission : Permission
    { }
}