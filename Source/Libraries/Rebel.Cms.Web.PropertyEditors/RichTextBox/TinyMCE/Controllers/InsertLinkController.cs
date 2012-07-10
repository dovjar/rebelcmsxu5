using System.Web.Mvc;
using System.Web.UI;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Editors;
using Rebel.Cms.Web.EmbeddedViewEngine;
using Rebel.Cms.Web.Model.BackOffice.Editors;

[assembly: WebResource("Rebel.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Resources.InsertLink.InsertLink.js", "application/x-javascript")]

namespace Rebel.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Controllers
{
    [Editor("C05B6ACC-C08E-497B-9078-5B23F67C0823")]
    [RebelEditor]
    public class InsertLinkController : AbstractEditorController
    {
        public InsertLinkController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        { }

        #region Actions

        /// <summary>
        /// Inserts the link.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult InsertLink()
        {
            return View(EmbeddedViewPath.Create("Rebel.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Views.InsertLink.InsertLink.cshtml, Rebel.Cms.Web.PropertyEditors"));
        }

        #endregion
    }
}
