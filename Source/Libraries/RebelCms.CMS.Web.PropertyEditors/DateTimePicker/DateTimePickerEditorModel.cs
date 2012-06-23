using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RebelCms.Cms.Web.EmbeddedViewEngine;
using System.ComponentModel.DataAnnotations;
using RebelCms.Cms.Web.Model.BackOffice;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;

namespace RebelCms.Cms.Web.PropertyEditors.DateTimePicker
{
    [EmbeddedView("RebelCms.Cms.Web.PropertyEditors.DateTimePicker.Views.DateTimePickerEditor.cshtml", "RebelCms.Cms.Web.PropertyEditors")]
    public class DateTimePickerEditorModel : EditorModel<DateTimePickerPreValueModel>, IValidatableObject
    {
        public DateTimePickerEditorModel(DateTimePickerPreValueModel preValueModel)
            : base(preValueModel)
        {}

       
        [ShowLabel(false)]
        public DateTime? Value { get; set; }

        public bool HasValue
        {
            get
            {
                return Value != null;
            }
        }

        public override void SetModelValues(IDictionary<string, object> serializedVal)
        {
            if (serializedVal.ContainsKey("Value") && serializedVal["Value"] != null && serializedVal["Value"] is DateTimeOffset)
                Value = ((DateTimeOffset)serializedVal["Value"]).DateTime;
            else
                base.SetModelValues(serializedVal);
            
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PreValueModel.IsRequired && Value == null)
            {
                yield return new ValidationResult("Value is required", new[] { "Value" });
            }
        }
    }
}
