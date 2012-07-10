using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Cms.Web.Surface;
using Rebel.Framework;

namespace Rebel.Tests.Cms.Stubs.Surface
{
    [Surface("E28D5B50-7BC6-4615-A3A9-BE2F29874E68")]
    internal class CustomSurfaceController : SurfaceController
    {
        public CustomSurfaceController(IBackOfficeRequestContext routableRequestContext)
            : base(routableRequestContext) { }

        public ActionResult Index(object id)
        {
            return null;
        }

        public ActionResult SomeAction(object id)
        {
            return null;
        }
    }
}
