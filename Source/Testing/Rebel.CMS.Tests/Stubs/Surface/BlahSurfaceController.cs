using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Surface;

namespace Rebel.Tests.Cms.Stubs.Surface
{
    [Surface("42DB035D-B52B-4F6D-9B06-581C1EC73C5F")]
    internal class BlahSurfaceController : SurfaceController
    {
        public BlahSurfaceController(IBackOfficeRequestContext routableRequestContext)
            : base(routableRequestContext) { }

        public ActionResult Index(object id)
        {
            return null;
        }

        public ActionResult AnotherAction(object id)
        {
            return null;
        }
    }
}