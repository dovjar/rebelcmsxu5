using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.PublicAccess, "Public Access", FixedPermissionTypes.EntityAction)]
    public class PublicAccessPermission : Permission
    { }
}