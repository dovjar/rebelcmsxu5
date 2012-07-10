using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Rebel.Cms.Web.EmbeddedViewEngine;
using Rebel.Cms.Web.Model.BackOffice;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Rebel.Cms.Web.PropertyEditors.Password
{
    [EmbeddedView("Rebel.Cms.Web.PropertyEditors.Password.Views.PasswordEditor.cshtml", "Rebel.Cms.Web.PropertyEditors")]
    public class PasswordEditorModel : EditorModel<PasswordPreValueModel>, IValidatableObject
    {
        public PasswordEditorModel(PasswordPreValueModel preValues) 
            : base(preValues)
        { }

        [ShowLabel(false)]
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
