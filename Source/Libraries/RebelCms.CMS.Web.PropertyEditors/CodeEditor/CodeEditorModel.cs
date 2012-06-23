using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RebelCms.Cms.Model.BackOffice;
using RebelCms.Cms.Model.BackOffice.PropertyEditors;
using RebelCms.Cms.Web.EmbeddedViewEngine;
using RebelCms.Cms.Web.PropertyEditors.Tags;

namespace RebelCms.Cms.Web.PropertyEditors.CodeEditor
{
    [EmbeddedView("RebelCms.Cms.Web.PropertyEditors.CodeEditor.Views.CodeEditor.cshtml", "RebelCms.Cms.Web.PropertyEditors")]
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
