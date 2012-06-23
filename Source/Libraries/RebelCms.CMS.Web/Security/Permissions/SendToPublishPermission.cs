using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.SendToPublish, "Send to Publish", FixedPermissionTypes.EntityAction)]
    public class SendToPublishPermission : Permission
    { }
}