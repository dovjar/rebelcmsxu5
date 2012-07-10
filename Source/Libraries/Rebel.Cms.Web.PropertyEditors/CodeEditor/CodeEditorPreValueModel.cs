using System.ComponentModel.DataAnnotations;
using Rebel.Cms.Model.BackOffice.PropertyEditors;
using Rebel.Cms.Web.EmbeddedViewEngine;

namespace Rebel.Cms.Web.PropertyEditors.CodeEditor
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
