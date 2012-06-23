using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Save, "Save", FixedPermissionTypes.EntityAction)]
    public class SavePermission : Permission
    { }
}