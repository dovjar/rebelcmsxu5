using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Editors;
using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Framework;

namespace RebelCms.Tests.Cms.Stubs.Editors
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