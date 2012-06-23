using System.ComponentModel.DataAnnotations;
using RebelCms.Cms.Model.BackOffice.PropertyEditors;
using RebelCms.Cms.Web.EmbeddedViewEngine;

namespace RebelCms.Cms.Web.PropertyEditors.CodeEditor
{
    public class CodeEditorPreValueModel : PreValueModel
    {
        public CodeEditorPreValueModel()
            : this(string.Empty)
        { }

        public CodeEditorPreValueModel(string preValues)
            : base(preValues)
        { }

        /// <summary>
        /// The language of code to display
        /// </summary>
        [UIHint("EnumDropDownList")]
        public CodeEditorLanguage CodeLanguage { get; set; }
    }
}
