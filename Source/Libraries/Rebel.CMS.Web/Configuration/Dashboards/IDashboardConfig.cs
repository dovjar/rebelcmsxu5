using System.Collections.Generic;

namespace Rebel.Cms.Web.Configuration.Dashboards
{
    public interface IDashboardConfig
    {
        IEnumerable<IDashboardGroup> Groups { get; }
    }
}