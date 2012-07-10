using System.Web.Mvc;
using Rebel.Cms.Packages.SystemInfo.Models;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Surface;
using Rebel.Framework;

namespace Rebel.Cms.Packages.SystemInfo.Controllers
{

    [Surface("98625300-6DF0-41AF-A432-83BD0232815A")]
    public class TestSurfaceController : SurfaceController
    {
        public TestSurfaceController(IRoutableRequestContext routableRequestContext)
            : base(routableRequestContext)
        {
        }

        [ChildActionOnly]
        public ContentResult HelloWorld()
        {
            return Content("Hello world");
        }

        [ChildActionOnly]
        public ContentResult HelloAgain()
        {
            return Content("Hello again");
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

                return CurrentRebelPage();
            }
            
            //return RedirectToRebelPage(HiveId.Parse("content://p__nhibernate/v__guid/00000000000000000000000000001049"));
            TempData["TestData"] = "blah";
            return RedirectToCurrentRebelPage();
        }
    }
}