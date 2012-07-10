using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Routing;
using Rebel.Framework;

using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Constants;

namespace Rebel.Cms.Web.Mvc.Controllers.BackOffice
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
