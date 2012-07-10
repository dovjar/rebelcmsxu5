using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Rebel.Cms.Web.EmbeddedViewEngine;
using Rebel.Cms.Web.Model.BackOffice;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;
using Rebel.Cms.Web.Mvc.Metadata;

namespace Rebel.Cms.Web.PropertyEditors.Label
{
    /// <summary>
    /// The model for the label property editor
    /// </summary>
    [EmbeddedView("Rebel.Cms.Web.PropertyEditors.Label.Views.LabelEditor.cshtml", "Rebel.Cms.Web.PropertyEditors")]
    public class LabelEditorModel : EditorModel
    {
        /// <summary>
        /// The Label value
        /// </summary>
        [ShowLabel(false)]
        public string Value { get; set; }
    }
}
