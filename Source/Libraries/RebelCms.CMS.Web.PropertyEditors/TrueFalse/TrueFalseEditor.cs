using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;

namespace RebelCms.Cms.Web.PropertyEditors.TrueFalse
{
    [PropertyEditor(CorePluginConstants.TrueFalsePropertyEditorId, "TrueFalse", "True/False", IsParameterEditor = true)]
    public class TrueFalseEditor : PropertyEditor<TrueFalseEditorModel>
    {
        public override TrueFalseEditorModel CreateEditorModel()
        {
            return new TrueFalseEditorModel();
        }
    }
}
