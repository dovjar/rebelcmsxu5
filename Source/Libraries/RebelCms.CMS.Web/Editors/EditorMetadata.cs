using System.Collections.Generic;
using RebelCms.Cms.Web.Model.BackOffice;

namespace RebelCms.Cms.Web.Editors
{
    public class EditorMetadata : ControllerPluginMetadata
    {
        public EditorMetadata(IDictionary<string, object> obj)
            : base(obj)
        {
        }


        /// <summary>
        /// Whether or not this is an built-in RebelCms editor
        /// </summary>
        public bool IsInternalRebelCmsEditor { get; set; }

        /// <summary>
        /// Flag to advertise if this Editor controller exposes child action dashboards
        /// </summary>
        public bool HasChildActionDashboards { get; set; }
        
    }
}
