using System.Web.Mvc;
using System.Web.UI;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Editors;
using RebelCms.Cms.Web.EmbeddedViewEngine;
using RebelCms.Cms.Web.Model.BackOffice.Editors;

[assembly: WebResource("RebelCms.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Resources.InsertAnchor.InsertAnchor.js", "application/x-javascript")]

namespace RebelCms.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Controllers
{
    [Editor("FD1C3D06-2044-4145-9EBF-F41C04B89D61")]
    [RebelCmsEditor]
    public class InsertAnchorController : AbstractEditorController
    {
        public InsertAnchorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        { }

        #region Actions

        /// <summary>
        /// Action to render the insert anchor dialog.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult InsertAnchor()
        {
            return View(EmbeddedViewPath.Create("RebelCms.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Views.InsertAnchor.InsertAnchor.cshtml, RebelCms.Cms.Web.PropertyEditors"));
        }

        #endregion
    }
}
