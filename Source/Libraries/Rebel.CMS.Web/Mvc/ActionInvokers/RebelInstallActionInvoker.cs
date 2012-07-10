using System.Linq;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Mvc.ActionFilters;

namespace Rebel.Cms.Web.Mvc.ActionInvokers
{
    /// <summary>
    /// Ensures that any filter of type RebelInstallAction get's an IApplicationContext set
    /// </summary>
    internal class RebelInstallActionInvoker : RebelBackOfficeActionInvoker
    {

        public RebelInstallActionInvoker(IBackOfficeRequestContext backOfficeRequestContext)
            : base(backOfficeRequestContext)
        {
            
        }

        protected override FilterInfo GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            base.GetFilters(controllerContext, actionDescriptor);

            var filters = base.GetFilters(controllerContext, actionDescriptor);
            foreach (var filter in
                filters.AuthorizationFilters.Where(filter => filter.GetType().Equals(typeof(RebelInstallAuthorizeAttribute))))
            {
                ((RebelInstallAuthorizeAttribute)filter).ApplicationContext = BackOfficeRequestContext.Application;
            }
            return filters;
        }

    }
}