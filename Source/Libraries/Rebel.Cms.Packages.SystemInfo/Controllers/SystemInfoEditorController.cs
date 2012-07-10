using System.Web.Mvc;
using Rebel.Cms.Packages.SystemInfo.Models;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Editors;

namespace Rebel.Cms.Packages.SystemInfo.Controllers
{
    [Editor("5D85C1EC-ED5C-451E-A53F-78CC95AA53A2")]
    public class SystemInfoEditorController : DashboardEditorController
    {
        public SystemInfoEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }


        public ActionResult RouteTable()
        {
            return View(new RouteTableModel());
        }

        public ActionResult PluginInfo()
        {
            return View();
        }

        public ActionResult Backup()
        {
            return View();
        }

        public ActionResult Permissions()
        {
            return View();
        }
    }
}
