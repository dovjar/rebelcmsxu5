using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.SendToPublish, "Send to Publish", FixedPermissionTypes.EntityAction, UserType.User)]
    public class SendToPublishPermission : Permission
    { }
}