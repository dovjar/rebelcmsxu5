using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using RebelCms.Cms.Web.EmbeddedViewEngine;
using RebelCms.Cms.Web.Model.BackOffice;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;

namespace RebelCms.Cms.Web.PropertyEditors.Password
{
    [EmbeddedView("RebelCms.Cms.Web.PropertyEditors.Password.Views.PasswordEditor.cshtml", "RebelCms.Cms.Web.PropertyEditors")]
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
