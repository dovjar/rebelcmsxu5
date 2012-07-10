using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Rebel.Cms.Model.BackOffice;
using Rebel.Cms.Model.BackOffice.PropertyEditors;
using Rebel.Cms.Web.EmbeddedViewEngine;
using Rebel.Cms.Web.PropertyEditors.Tags;

namespace Rebel.Cms.Web.PropertyEditors.CodeEditor
{
    [EmbeddedView("Rebel.Cms.Web.PropertyEditors.CodeEditor.Views.CodeEditor.cshtml", "Rebel.Cms.Web.PropertyEditors")]
    public class CodeEditorModel : EditorModel<CodeEditorPreValueModel>
    {
        public CodeEditorModel(CodeEditorPreValueModel preValueModel)
            : base(preValueModel)
        { }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [ShowLabel(false)]
        public string Value { get; set; }

    
    }
}
