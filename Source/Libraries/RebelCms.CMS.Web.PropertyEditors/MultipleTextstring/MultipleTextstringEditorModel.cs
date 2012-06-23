using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using RebelCms.Cms.Web.EmbeddedViewEngine;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;

namespace RebelCms.Cms.Web.PropertyEditors.MultipleTextstring
{
    [EmbeddedView("RebelCms.Cms.Web.PropertyEditors.MultipleTextstring.Views.MultipleTextstringEditor.cshtml", "RebelCms.Cms.Web.PropertyEditors")]
    public class MultipleTextstringEditorModel : EditorModel<MultipleTextstringPreValueModel>, IValidatableObject
    {
        public MultipleTextstringEditorModel(MultipleTextstringPreValueModel preValues) 
            : base(preValues)
        { }

        public string Value { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PreValueModel.IsRequired && string.IsNullOrWhiteSpace(Value))
            {
                yield return new ValidationResult("Value is required", new[] { "Value" });
            }
        }
    }
}
