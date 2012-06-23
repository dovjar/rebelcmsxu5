using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Language, "Language", FixedPermissionTypes.EntityAction)]
    public class LanguagePermission : Permission
    { }
}