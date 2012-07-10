namespace Rebel.Cms.Packages.SnapshotDashboard.Models
{
    public class SnapshotResultModel
    {
        public bool SnapshotCreated { get; set; }
        public string SnapshotLocation { get; set; }
        public string NotificationTitle { get; set; }
        public string NotificationMessage { get; set; }
        public string NotificationType { get; set; }
    }
}