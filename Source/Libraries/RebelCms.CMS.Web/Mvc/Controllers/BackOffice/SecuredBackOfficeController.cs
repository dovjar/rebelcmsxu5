using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Mvc.Controllers.BackOffice
{
    /// <summary>
    /// Ensures that the user is authorized to access any actions on the controller
    /// </summary>
    [RebelCmsAuthorize(Permissions = new[]{ FixedPermissionIds.BackOfficeAccess }, Order = 10)]
    public abstract class SecuredBackOfficeController : BackOfficeController
    {
        protected SecuredBackOfficeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        { }
    }
}