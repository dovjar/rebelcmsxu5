using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model.BackOffice;

namespace RebelCms.Cms.Web.Dashboards
{
    public abstract class DashboardViewPage : WebViewPage<DashboardViewModel>, IRequiresBackOfficeRequestContext
    {

        public IBackOfficeRequestContext BackOfficeRequestContext { get; set; }

    }
}