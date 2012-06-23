using System.Web.Mvc;
using RebelCms.Cms.Packages.SystemInfo.Models;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Surface;
using RebelCms.Framework;

namespace RebelCms.Cms.Packages.SystemInfo.Controllers
{

    [Surface("98625300-6DF0-41AF-A432-83BD0232815A")]
    public class TestSurfaceController : SurfaceController
    {
        public TestSurfaceController(IRoutableRequestContext routableRequestContext)
            : base(routableRequestContext)
        {
        }

        [ChildActionOnly]
        public ActionResult HelloWorld()
        {
            return Content("Hello world");
        }

        [ChildActionOnly]
        public PartialViewResult DisplayForm(string stringToDisplay)
        {
            return PartialView(new TestSurfaceFormModel() {SomeString = stringToDisplay});
        }

        [HttpPost] 
        public ActionResult HandleFormSubmit([Bind(Prefix = "MyTestForm")]TestSurfaceFormModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.TestData = "hello";                

                return CurrentRebelCmsPage();
            }
            
            //return RedirectToRebelCmsPage(HiveId.Parse("content://p__nhibernate/v__guid/00000000000000000000000000001049"));
            TempData["TestData"] = "blah";
            return RedirectToCurrentRebelCmsPage();
        }
    }
}