using Rebel.Cms.Model.BackOffice.PropertyEditors;
using Rebel.Cms.Web.PropertyEditors.Tags;

namespace Rebel.Cms.Web.PropertyEditors.CodeEditor
{
    [PropertyEditor("5B68D5D9-3D6C-4DF5-B133-4F49FAC0924A", "CodeEditor", "Code Editor")]
    public class CodeEditor : PropertyEditor<CodeEditorModel, CodeEditorPreValueModel>
    {
        public override CodeEditorModel CreateEditorModel(CodeEditorPreValueModel preValues)
        {
            return new CodeEditorModel(preValues);
        }

        public override CodeEditorPreValueModel CreatePreValueEditorModel(string preValues)
        {
            return new CodeEditorPreValueModel(preValues);
        }
    }
}
