using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Rebel.Cms.Web.EmbeddedViewEngine;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Rebel.Cms.Web.PropertyEditors.MultipleTextstring
{
    [EmbeddedView("Rebel.Cms.Web.PropertyEditors.MultipleTextstring.Views.MultipleTextstringEditor.cshtml", "Rebel.Cms.Web.PropertyEditors")]
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
