using System.Web.Mvc;
using Umbraco.Cms.Web.Mvc.ModelBinders.BackOffice;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
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