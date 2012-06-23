using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Translate, "Translate", FixedPermissionTypes.EntityAction)]
    public class TranslatePermission : Permission
    { }
}