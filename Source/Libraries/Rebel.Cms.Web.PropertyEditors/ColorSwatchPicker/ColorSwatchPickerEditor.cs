using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;


[assembly: WebResource("Rebel.Cms.Web.PropertyEditors.ColorSwatchPicker.Resources.ColorSwatchPicker.js", "application/x-javascript")]
[assembly: WebResource("Rebel.Cms.Web.PropertyEditors.ColorSwatchPicker.Resources.ColorSwatchPickerPrevalueEditor.js", "application/x-javascript")]
[assembly: WebResource("Rebel.Cms.Web.PropertyEditors.ColorSwatchPicker.Resources.ColorSwatchPicker.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("Rebel.Cms.Web.PropertyEditors.ColorSwatchPicker.Resources.ColorSwatchPickerPrevalueEditor.css", "text/css")]
[assembly: WebResource("Rebel.Cms.Web.PropertyEditors.ColorSwatchPicker.Resources.select.png", "image/png")]

namespace Rebel.Cms.Web.PropertyEditors.ColorSwatchPicker
{
    [PropertyEditor(CorePluginConstants.ColorSwatchPickerPropertyEditorId, "ColorSwatchPicker", "Color Swatch Picker")]
    public class ColorSwatchPickerEditor : PropertyEditor<ColorSwatchPickerEditorModel, ColorSwatchPickerPreValueModel>
    {
        public override ColorSwatchPickerEditorModel CreateEditorModel(ColorSwatchPickerPreValueModel preValues)
        {
            return new ColorSwatchPickerEditorModel(preValues);
        }

        public override ColorSwatchPickerPreValueModel CreatePreValueEditorModel()
        {
            return new ColorSwatchPickerPreValueModel();
        }
    }
}
