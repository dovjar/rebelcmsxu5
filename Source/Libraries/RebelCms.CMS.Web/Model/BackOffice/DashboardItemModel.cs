using System.Collections.Generic;
using RebelCms.Cms.Web.Configuration.Dashboards;
using RebelCms.Framework;

namespace RebelCms.Cms.Web.Model.BackOffice
{
    public class DashboardItemModel
    {
        public HiveId TabId { get; set; }

        public string ViewName { get; set; }

        public IEnumerable<IDashboardMatch> Matches { get; set; }

        public DashboardType DashboardType { get; set; }
    }
}