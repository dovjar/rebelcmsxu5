using System.Collections.Generic;
using Rebel.Cms.Web.Model.BackOffice.Editors;

namespace Rebel.Cms.Web.Model.BackOffice
{
    public class DashboardApplicationModel
    {
        public IEnumerable<Tab> Tabs { get; set; }

        public IEnumerable<DashboardItemModel> Dashboards { get; set; }
    }
}