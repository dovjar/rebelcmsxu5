using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Language, "Language", FixedPermissionTypes.EntityAction, UserType.User)]
    public class LanguagePermission : Permission
    { }
}