using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;

[assembly: WebResource("Rebel.Cms.Web.PropertyEditors.ListPicker.Resources.ListPickerPrevalueEditor.js", "application/x-javascript")]
[assembly: WebResource("Rebel.Cms.Web.PropertyEditors.ListPicker.Resources.ListPickerPrevalueEditor.css", "text/css")]

namespace Rebel.Cms.Web.PropertyEditors.ListPicker
{
    [PropertyEditor(CorePluginConstants.ListPickerPropertyEditorId, "ListPicker", "List Picker")]
    public class ListPickerEditor : PropertyEditor<ListPickerEditorModel, ListPickerPreValueModel>
    {
        public override ListPickerEditorModel CreateEditorModel(ListPickerPreValueModel preValues)
        {
            return new ListPickerEditorModel(preValues);
        }

        public override ListPickerPreValueModel CreatePreValueEditorModel()
        {
            return new ListPickerPreValueModel();
        }
    }
}
