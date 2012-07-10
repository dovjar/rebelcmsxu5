using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace Rebel.Cms.Packages.SystemInfo.Models
{
    [Bind(Exclude = "Routes")]
    public class RouteTableModel
    {
        public string TestUrl { get; set; }

        [ReadOnly(true)]
        [Editable(false)]
        public RouteCollection Routes { get; set; }
    }
}
