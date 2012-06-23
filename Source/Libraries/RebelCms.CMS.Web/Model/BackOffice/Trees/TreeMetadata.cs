using System.Collections.Generic;

namespace RebelCms.Cms.Web.Model.BackOffice.Trees
{

    /// <summary>
    /// A meta data class to explain a tree controller
    /// </summary>
    public class TreeMetadata : ControllerPluginMetadata
    {
        public TreeMetadata(IDictionary<string, object> obj)
            : base(obj)
        {
        }

        /// <summary>
        /// The title to show for the root tree node
        /// </summary>
        public string TreeTitle { get; set; }

        /// <summary>
        /// Whether or not this is a built-in RebelCms tree
        /// </summary>
        public bool IsInternalRebelCmsTree { get; set; }

    }
}
