using System.Web.Mvc;
using Rebel.Cms.Web.Mvc.ModelBinders.BackOffice;
using Rebel.Framework;

namespace Rebel.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// Represents the view model used to render a media type editor
    /// </summary>
    [ModelBinder(typeof(DocumentTypeModelBinder))]
    public class MediaTypeEditorModel : AbstractSchemaEditorModel
    {
        public MediaTypeEditorModel()
        {
            
        }

        public MediaTypeEditorModel(HiveId id)
            : base(id)
        {
        }
    }
}