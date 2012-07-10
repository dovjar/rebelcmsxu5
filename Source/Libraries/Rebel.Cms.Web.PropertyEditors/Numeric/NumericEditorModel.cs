using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Rebel.Cms.Web.EmbeddedViewEngine;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Rebel.Cms.Web.PropertyEditors.Numeric
{
    [EmbeddedView("Rebel.Cms.Web.PropertyEditors.Numeric.Views.NumericEditor.cshtml", "Rebel.Cms.Web.PropertyEditors")]
    public class NumericEditorModel : EditorModel<NumericPreValueModel>, IValidatableObject
    {
        public NumericEditorModel(NumericPreValueModel preValues) 
            : base(preValues)
        { }

        public decimal ValueAsDecimal { get; set; }

        public int ValueAsInteger { 
            get { return (int)ValueAsDecimal; }
            set { ValueAsDecimal = value; }
        }

        public override IDictionary<string, object> GetSerializedValue()
        {
            return new Dictionary<string, object> {{"Value", ValueAsDecimal}};
        }

        public override void SetModelValues(IDictionary<string, object> serializedVal)
        {
            decimal value = 0;
            if (serializedVal.ContainsKey("Value"))
                decimal.TryParse(serializedVal["Value"].ToString(), out value);
            ValueAsDecimal = value;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PreValueModel.IsRequired && ValueAsDecimal == 0)
            {
                yield return new ValidationResult("Value is required", new[] { "ValueAsDecimal" });
            }
        }
    }
}
