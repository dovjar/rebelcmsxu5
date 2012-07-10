using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Mvc.Controllers.BackOffice
{
    /// <summary>
    /// Ensures that the user is authorized to access any actions on the controller
    /// </summary>
    [RebelAuthorize(Permissions = new[]{ FixedPermissionIds.BackOfficeAccess }, Order = 10)]
    public abstract class SecuredBackOfficeController : BackOfficeController
    {
        protected SecuredBackOfficeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        { }
    }
}