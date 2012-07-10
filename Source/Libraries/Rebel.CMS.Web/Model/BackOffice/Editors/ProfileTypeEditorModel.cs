using System.Web.Mvc;
using Rebel.Cms.Web.Mvc.ModelBinders.BackOffice;
using Rebel.Framework;

namespace Rebel.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// Represents the view model used to render a media type editor
    /// </summary>
    [ModelBinder(typeof(DocumentTypeModelBinder))]
    public class ProfileTypeEditorModel : AbstractSchemaEditorModel
    {
        public ProfileTypeEditorModel()
        {
            
        }

        public ProfileTypeEditorModel(HiveId id)
            : base(id)
        {
        }
    }
}