using System;

namespace RebelCms.Cms.Web.Configuration.Dashboards
{
    public interface IDashboardMatchFilter
    {
        Type MatchFilterType { get; }
        string DataValue { get; }
    }
}