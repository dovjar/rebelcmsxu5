using System.Web.Mvc;
using RebelCms.Cms.Web.Model.BackOffice.ParameterEditors;
using ParameterEditorModel = RebelCms.Cms.Web.Model.BackOffice.ParameterEditors.EditorModel;

namespace RebelCms.Cms.Web.Model.BackOffice.Editors
{
    public class MacroParameterModel : IMetadataAware
    {
        /// <summary>
        /// The alias of the parameter
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// The name of the parameter
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The parameter editor model of the parameter
        /// </summary>
        public ParameterEditorModel ParameterEditorModel { get; set; }

        /// <summary>
        /// When implemented in a class, provides metadata to the model metadata creation process.
        /// </summary>
        /// <param name="metadata">The model metadata.</param>
        public void OnMetadataCreated(ModelMetadata metadata)
        {
            metadata.DisplayName = Name;
        }
    }
}
