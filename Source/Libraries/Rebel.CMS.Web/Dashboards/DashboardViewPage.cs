using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice;

namespace Rebel.Cms.Web.Dashboards
{
    public abstract class DashboardViewPage : WebViewPage<DashboardViewModel>, IRequiresBackOfficeRequestContext
    {

        public IBackOfficeRequestContext BackOfficeRequestContext { get; set; }

    }
}