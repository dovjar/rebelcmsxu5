using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Audit, "Audit", FixedPermissionTypes.EntityAction, UserType.User)]
    public class AuditPermission : Permission
    { }
}
