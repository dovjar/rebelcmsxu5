using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;
using RebelCms.Cms.Web.PropertyEditors.Numeric;

[assembly: WebResource("RebelCms.Cms.Web.PropertyEditors.Numeric.Resources.NumericPropertyEditor.js", "application/x-javascript")]

namespace RebelCms.Cms.Web.PropertyEditors.Numeric
{
    [PropertyEditor(CorePluginConstants.NumericPropertyEditorId, "Numeric", "Numeric", IsParameterEditor = true)]
    public class NumericEditor : PropertyEditor<NumericEditorModel, NumericPreValueModel>
    {
        public override NumericEditorModel CreateEditorModel(NumericPreValueModel preValues)
        {
            return new NumericEditorModel(preValues);
        }

        public override NumericPreValueModel CreatePreValueEditorModel()
        {
            return new NumericPreValueModel();
        }
    }
}
