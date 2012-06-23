using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RebelCms.Cms.Web.Model.BackOffice;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;

namespace RebelCms.Cms.Web.PropertyEditors.TrueFalse
{
    public class TrueFalseEditorModel : EditorModel
    {
        [ShowLabel(false)]
        public bool Value { get; set; }
    }
}
