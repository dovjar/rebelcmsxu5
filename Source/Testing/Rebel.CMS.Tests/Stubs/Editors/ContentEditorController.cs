using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Editors;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Framework;

namespace Rebel.Tests.Cms.Stubs.Editors
{
    /// <summary>
    /// An editor for testing routes when there are multiple editors with the same name (as plugins)
    /// </summary>
    [Editor("3BD8E124-6F2B-4C5C-96F5-9B016EF479E7")]
    internal class ContentEditorController : StandardEditorController
    {
        public ContentEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext) { }

        public override ActionResult Edit(HiveId? id)
        {
            return null;
        }

        public ActionResult EditForm(HiveId? id)
        {
            return null;
        }
    }
}