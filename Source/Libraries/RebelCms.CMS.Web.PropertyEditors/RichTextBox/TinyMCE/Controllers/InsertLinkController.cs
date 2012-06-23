using System.Web.Mvc;
using System.Web.UI;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Editors;
using RebelCms.Cms.Web.EmbeddedViewEngine;
using RebelCms.Cms.Web.Model.BackOffice.Editors;

[assembly: WebResource("RebelCms.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Resources.InsertLink.InsertLink.js", "application/x-javascript")]

namespace RebelCms.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Controllers
{
    [Editor("C05B6ACC-C08E-497B-9078-5B23F67C0823")]
    [RebelCmsEditor]
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
            return View(EmbeddedViewPath.Create("RebelCms.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Views.InsertLink.InsertLink.cshtml, RebelCms.Cms.Web.PropertyEditors"));
        }

        #endregion
    }
}
