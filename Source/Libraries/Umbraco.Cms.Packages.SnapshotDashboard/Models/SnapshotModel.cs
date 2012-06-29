using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Packages.SnapshotDashboard.Models
{
    public class SnapshotModel
    {
        [Display(Name = "Include Content")]
        public bool IncludeContent { get; set; }

        [Display(Name = "Include Media")]
        public bool IncludeMedia { get; set; }

        [Display(Name = "Include Document Types")]
        public bool IncludeDocumentTypes { get; set; }
    }
}