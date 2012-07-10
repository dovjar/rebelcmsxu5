using System;
using System.Collections.Generic;

namespace Rebel.Cms.Web.Configuration.Dashboards
{
    public interface IDashboardMatch
    {
        Type MatchType { get; }
        string Condition { get; }
        IEnumerable<IDashboardMatchFilter> Filters { get; } 
    }
}