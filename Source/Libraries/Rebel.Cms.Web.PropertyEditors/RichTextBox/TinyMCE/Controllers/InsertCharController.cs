using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Editors;
using Rebel.Cms.Web.EmbeddedViewEngine;
using Rebel.Cms.Web.Model.BackOffice.Editors;

namespace Rebel.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Controllers
{
    [Editor("70AEA379-A639-43DE-81C9-FE5C5B275213")]
    [RebelEditor]
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
            return View(EmbeddedViewPath.Create("Rebel.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Views.InsertChar.InsertChar.cshtml, Rebel.Cms.Web.PropertyEditors"));
        }

        #endregion
    }
}
