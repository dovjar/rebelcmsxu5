using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Cms.Web.Model.BackOffice;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Rebel.Cms.Web.PropertyEditors.TrueFalse
{
    public class TrueFalseEditorModel : EditorModel
    {
        [ShowLabel(false)]
        public bool Value { get; set; }
    }
}
