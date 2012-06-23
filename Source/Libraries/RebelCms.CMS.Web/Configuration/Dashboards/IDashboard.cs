using RebelCms.Cms.Web.Model.BackOffice;

namespace RebelCms.Cms.Web.Configuration.Dashboards
{
    public interface IDashboard
    {
        string TabName { get; }
        DashboardType DashboardType { get; }
        string Name { get; }
    }
}