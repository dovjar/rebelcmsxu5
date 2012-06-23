using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;

[assembly: WebResource("RebelCms.Cms.Web.PropertyEditors.MultipleTextstring.Resources.MultipleTextstringPropertyEditor.js", "application/x-javascript")]

namespace RebelCms.Cms.Web.PropertyEditors.MultipleTextstring
{
    [PropertyEditor("F0416B3A-86B4-4453-9F0B-87471E5B244A", "MultipleTextstring", "Multiple Textstring")]
    public class MultipleTextstringEditor : PropertyEditor<MultipleTextstringEditorModel, MultipleTextstringPreValueModel>
    {
        public override MultipleTextstringEditorModel CreateEditorModel(MultipleTextstringPreValueModel preValues)
        {
            return new MultipleTextstringEditorModel(preValues);
        }

        public override MultipleTextstringPreValueModel CreatePreValueEditorModel()
        {
            return new MultipleTextstringPreValueModel();
        }
    }
}
