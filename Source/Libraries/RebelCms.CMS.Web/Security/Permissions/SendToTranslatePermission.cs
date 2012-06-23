using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.SendToTranslate, "Send to Translate", FixedPermissionTypes.EntityAction)]
    public class SendToTranslatePermission : Permission
    { }
}