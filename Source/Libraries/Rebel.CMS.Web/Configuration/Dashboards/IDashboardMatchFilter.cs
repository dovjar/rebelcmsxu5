using System;

namespace Rebel.Cms.Web.Configuration.Dashboards
{
    public interface IDashboardMatchFilter
    {
        Type MatchFilterType { get; }
        string DataValue { get; }
    }
}