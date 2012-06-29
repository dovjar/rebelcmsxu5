using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Unpublish, "Unpublish", FixedPermissionTypes.EntityAction, UserType.User)]
    public class UnpublishPermission : Permission
    { }
}