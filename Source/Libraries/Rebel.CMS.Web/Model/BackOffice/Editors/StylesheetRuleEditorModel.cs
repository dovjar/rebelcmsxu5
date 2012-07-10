using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Rebel.Cms.Web.Model.BackOffice.UIElements;
using Rebel.Framework;

namespace Rebel.Cms.Web.Model.BackOffice.Editors
{
    public class StylesheetRuleEditorModel : EditorModel
    {
        public StylesheetRuleEditorModel() 
        {
            PopulateUIElements();
        }

        [Required]
        public string Selector { get; set; }

        public string Styles { get; set; }

        protected void PopulateUIElements()
        {
            UIElements.Clear();
            UIElements.Add(new SaveButtonUIElement());
        }
    }
}
