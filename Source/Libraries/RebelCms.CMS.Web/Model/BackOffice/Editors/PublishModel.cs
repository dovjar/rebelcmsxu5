using System.ComponentModel;
using System.Web.Mvc;
using RebelCms.Framework;

namespace RebelCms.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// Model representing the publish dialog
    /// </summary>
    [Bind(Exclude = "Name")]
    public class PublishModel : DialogModel
    {
        public HiveId Id { get; set; }
        
        [ReadOnly(true)]
        public string Name { get; set; }

        public bool IncludeChildren { get; set; }

        public bool IncludeUnpublishedChildren { get; set; }

    }
}