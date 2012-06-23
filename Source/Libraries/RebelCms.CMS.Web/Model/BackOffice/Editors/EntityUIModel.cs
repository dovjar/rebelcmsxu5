using System.Collections.Generic;
using RebelCms.Framework;

namespace RebelCms.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// Represents a basic RebelCms UI model with tabs
    /// </summary>
    public abstract class EntityUIModel : CoreEntityModel
    {
  
        protected EntityUIModel()
        {  
            Tabs = new HashSet<Tab>();
        }

        /// <summary>
        /// Tabs used to organize editing
        /// </summary>        
        public HashSet<Tab> Tabs { get; set; }

    }
}
