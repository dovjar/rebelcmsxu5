using System.Collections.Generic;
using RebelCms.Cms.Web.Model.BackOffice.Editors;

namespace RebelCms.Cms.Web.Model.BackOffice
{
    public class DashboardApplicationModel
    {
        public IEnumerable<Tab> Tabs { get; set; }

        public IEnumerable<DashboardItemModel> Dashboards { get; set; }
    }
}