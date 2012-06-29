using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Publish, "Publish", FixedPermissionTypes.EntityAction, UserType.User)]
    public class PublishPermission : Permission
    { }
}