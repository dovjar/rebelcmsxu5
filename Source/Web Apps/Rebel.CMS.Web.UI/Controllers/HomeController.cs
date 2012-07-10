using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Rebel.Cms.Web.Mvc.Controllers;

namespace Rebel.Cms.Web.UI.Controllers
{
    using Rebel.Cms.Web.Context;
    using Rebel.Framework.Diagnostics;

    public class HomeController : Controller
    {
        /// <summary>
        /// Default Action when Rebel is not installed, however, if you would like a 'HomeController', then you can just override this one and
        /// replace 'Index'
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult Index()
        {
            ViewBag.Message = "Welcome to Rebel 5";
            ViewBag.IsInstalled = false;
            try
            {
                var appContext = DependencyResolver.Current.GetService<IRebelApplicationContext>();
                if (appContext != null)
                {
                    if (appContext.AllProvidersInstalled())
                    {
                        ViewBag.IsInstalled = true;
                        ViewBag.Message = "Oops, no content published yet for this domain!";
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<HomeController>("Couldn't get the installation status, this is normal if Rebel 5 isn't yet installed", ex);
                ViewBag.IsInstalled = false;
            }

            
            return View();
        }
    }
}
