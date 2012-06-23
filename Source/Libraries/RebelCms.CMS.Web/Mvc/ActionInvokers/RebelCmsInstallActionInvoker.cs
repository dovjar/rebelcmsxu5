using System.Linq;
using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Mvc.ActionFilters;

namespace RebelCms.Cms.Web.Mvc.ActionInvokers
{
    /// <summary>
    /// Ensures that any filter of type RebelCmsInstallAction get's an IApplicationContext set
    /// </summary>
    internal class RebelCmsInstallActionInvoker : RebelCmsBackOfficeActionInvoker
    {

        public RebelCmsInstallActionInvoker(IBackOfficeRequestContext backOfficeRequestContext)
            : base(backOfficeRequestContext)
        {
            
        }

        protected override FilterInfo GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            base.GetFilters(controllerContext, actionDescriptor);

            var filters = base.GetFilters(controllerContext, actionDescriptor);
            foreach (var filter in
                filters.AuthorizationFilters.Where(filter => filter.GetType().Equals(typeof(RebelCmsInstallAuthorizeAttribute))))
            {
                ((RebelCmsInstallAuthorizeAttribute)filter).ApplicationContext = BackOfficeRequestContext.Application;
            }
            return filters;
        }

    }
}