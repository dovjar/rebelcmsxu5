using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Cms.Web.Mvc.Controllers;

namespace Umbraco.Cms.Web.UI.Controllers
{
    using Umbraco.Cms.Web.Context;
    using Umbraco.Framework.Diagnostics;

    public class HomeController : Controller
    {
        /// <summary>
        /// Default Action when Umbraco is not installed, however, if you would like a 'HomeController', then you can just override this one and
        /// replace 'Index'
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult Index()
        {
            ViewBag.Message = "Welcome to Umbraco 5";
            ViewBag.IsInstalled = false;
            try
            {
                var appContext = DependencyResolver.Current.GetService<IUmbracoApplicationContext>();
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
                LogHelper.Error<HomeController>("Couldn't get the installation status, this is normal if Umbraco 5 isn't yet installed", ex);
                ViewBag.IsInstalled = false;
            }

            
            return View();
        }
    }
}
