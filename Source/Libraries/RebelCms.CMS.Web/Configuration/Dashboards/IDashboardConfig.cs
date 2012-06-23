using System.Collections.Generic;

namespace RebelCms.Cms.Web.Configuration.Dashboards
{
    public interface IDashboardConfig
    {
        IEnumerable<IDashboardGroup> Groups { get; }
    }
}