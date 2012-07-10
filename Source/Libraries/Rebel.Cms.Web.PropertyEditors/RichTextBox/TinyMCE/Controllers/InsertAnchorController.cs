using System.Web.Mvc;
using System.Web.UI;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Editors;
using Rebel.Cms.Web.EmbeddedViewEngine;
using Rebel.Cms.Web.Model.BackOffice.Editors;

[assembly: WebResource("Rebel.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Resources.InsertAnchor.InsertAnchor.js", "application/x-javascript")]

namespace Rebel.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Controllers
{
    [Editor("FD1C3D06-2044-4145-9EBF-F41C04B89D61")]
    [RebelEditor]
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
            return View(EmbeddedViewPath.Create("Rebel.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Views.InsertAnchor.InsertAnchor.cshtml, Rebel.Cms.Web.PropertyEditors"));
        }

        #endregion
    }
}
