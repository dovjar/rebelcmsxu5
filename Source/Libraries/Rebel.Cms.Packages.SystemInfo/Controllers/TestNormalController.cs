using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Rebel.Cms.Packages.SystemInfo.Controllers
{
    public class TestNormalController : Controller
    {

        public ActionResult Index( )
        {
            return Content("hello");
        }

    }
}
