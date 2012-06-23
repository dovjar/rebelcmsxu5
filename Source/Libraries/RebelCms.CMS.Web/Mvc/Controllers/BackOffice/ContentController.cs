using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Routing;
using RebelCms.Framework;

using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Constants;

namespace RebelCms.Cms.Web.Mvc.Controllers.BackOffice
{
    public class ContentController : BackOfficeController
    {
        public ContentController(IBackOfficeRequestContext requestContext) 
            : base(requestContext)
        { }

        /// <summary>
        /// Gets the nices URL for .
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public JsonResult NiceUrl(HiveId id)
        {
            return Json(new
                {
                    niceUrl = BackOfficeRequestContext.RoutingEngine.GetUrl(id)
                });
        }
    }
}
