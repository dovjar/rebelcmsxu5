using System.Collections.Generic;
using Rebel.Cms.Web.Configuration.Dashboards;
using Rebel.Framework;

namespace Rebel.Cms.Web.Model.BackOffice
{
    public class DashboardItemModel
    {
        public HiveId TabId { get; set; }

        public string ViewName { get; set; }

        public IEnumerable<IDashboardMatch> Matches { get; set; }

        public DashboardType DashboardType { get; set; }
    }
}