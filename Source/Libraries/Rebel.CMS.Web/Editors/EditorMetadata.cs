using System.Collections.Generic;
using Rebel.Cms.Web.Model.BackOffice;

namespace Rebel.Cms.Web.Editors
{
    public class EditorMetadata : ControllerPluginMetadata
    {
        public EditorMetadata(IDictionary<string, object> obj)
            : base(obj)
        {
        }


        /// <summary>
        /// Whether or not this is an built-in Rebel editor
        /// </summary>
        public bool IsInternalRebelEditor { get; set; }

        /// <summary>
        /// Flag to advertise if this Editor controller exposes child action dashboards
        /// </summary>
        public bool HasChildActionDashboards { get; set; }
        
    }
}
