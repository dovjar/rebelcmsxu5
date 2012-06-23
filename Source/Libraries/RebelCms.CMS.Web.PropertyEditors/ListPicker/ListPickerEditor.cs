using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;

[assembly: WebResource("RebelCms.Cms.Web.PropertyEditors.ListPicker.Resources.ListPickerPrevalueEditor.js", "application/x-javascript")]
[assembly: WebResource("RebelCms.Cms.Web.PropertyEditors.ListPicker.Resources.ListPickerPrevalueEditor.css", "text/css")]

namespace RebelCms.Cms.Web.PropertyEditors.ListPicker
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
