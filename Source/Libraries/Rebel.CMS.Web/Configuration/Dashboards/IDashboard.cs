using Rebel.Cms.Web.Model.BackOffice;

namespace Rebel.Cms.Web.Configuration.Dashboards
{
    public interface IDashboard
    {
        string TabName { get; }
        DashboardType DashboardType { get; }
        string Name { get; }
    }
}