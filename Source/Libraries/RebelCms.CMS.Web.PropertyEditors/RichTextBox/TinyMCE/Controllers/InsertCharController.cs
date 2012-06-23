using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Editors;
using RebelCms.Cms.Web.EmbeddedViewEngine;
using RebelCms.Cms.Web.Model.BackOffice.Editors;

namespace RebelCms.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Controllers
{
    [Editor("70AEA379-A639-43DE-81C9-FE5C5B275213")]
    [RebelCmsEditor]
    public class InsertCharController : AbstractEditorController
    {
        public InsertCharController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        { }

        #region Actions

        /// <summary>
        /// Action to render the insert char dialog.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult InsertChar()
        {
            return View(EmbeddedViewPath.Create("RebelCms.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Views.InsertChar.InsertChar.cshtml, RebelCms.Cms.Web.PropertyEditors"));
        }

        #endregion
    }
}
