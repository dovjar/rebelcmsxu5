using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace RebelCms.Cms.Web.Mvc
{
    public class HierarchicalSelectListItem : SelectListItem
    {
        public string[] ParentValues { get; set; }
    }
}
