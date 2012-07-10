using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Rebel.Cms.Web.EmbeddedViewEngine;
using System.Xml.Linq;
using Rebel.Cms.Web.Model.BackOffice;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Rebel.Cms.Web.PropertyEditors.ListPicker
{
    [ModelBinder(typeof(ListPickerEditorModelBinder))]
    [EmbeddedView("Rebel.Cms.Web.PropertyEditors.ListPicker.Views.ListPickerEditor.cshtml", "Rebel.Cms.Web.PropertyEditors")]
    public class ListPickerEditorModel : EditorModel<ListPickerPreValueModel>, IValidatableObject
    {
        public ListPickerEditorModel(ListPickerPreValueModel preValueModel)
            : base(preValueModel)
        { }

        [ShowLabel(false)]
        public IEnumerable<string> Value { get; set; }

        public override IDictionary<string, object> GetSerializedValue()
        {
            var vals = new Dictionary<string, object>();

            var count = 0;
            foreach (var item in Value)
            {
                vals.Add("val" + count, item);
                count++;
            }

            return vals;
        }

        public override void SetModelValues(IDictionary<string, object> serializedVal)
        {
            Value = serializedVal.Select(item => item.Value.ToString()).ToList();
        }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PreValueModel.IsRequired && (Value == null || Value.Count() == 0))
            {
                yield return new ValidationResult("Value is required", new[] { "Value" });
            }
        }
    }
}
