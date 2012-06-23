using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RebelCms.Cms.Web.Mvc.Controllers;

namespace RebelCms.Cms.Web.UI.Controllers
{
    using RebelCms.Cms.Web.Context;
    using RebelCms.Framework.Diagnostics;

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to RebelCms 5";
            ViewBag.IsInstalled = false;
            try
            {
                var appContext = DependencyResolver.Current.GetService<IRebelCmsApplicationContext>();
                if (appContext != null)
                {
                    if (appContext.AllProvidersInstalled())
                    {
                        ViewBag.IsInstalled = true;
                        ViewBag.Message = "Oops, no content published yet!";
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<HomeController>("Couldn't get the installation status, this is normal if RebelCms 5 isn't yet installed", ex);
                ViewBag.IsInstalled = false;
            }

            return View();
        }
    }
}
