using System;
using System.Collections.Generic;
using RebelCms.Cms.Web.Model.BackOffice.Trees;

namespace RebelCms.Cms.Web.Model.BackOffice
{

    /// <summary>
    /// The data model supplied to the default back office application
    /// </summary>
    public class BackOfficeDefaultModel
    {        

        /// <summary>
        /// The default dashboard editor to render
        /// </summary>
        //public Type DefaultDashboardEditor { get; set; }
        public TreeRenderModel TreeModel { get; set; }
        public IEnumerable<ApplicationTrayModel> Applications { get; set; }

        /// <summary>
        /// All available menu items to render their definitions
        /// </summary>
        public IEnumerable<MenuItem> MenuItems { get; set; }

        public string CurrentApplicationAlias { get; set; }
    }
}
