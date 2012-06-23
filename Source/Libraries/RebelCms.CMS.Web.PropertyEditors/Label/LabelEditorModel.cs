using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using RebelCms.Cms.Web.EmbeddedViewEngine;
using RebelCms.Cms.Web.Model.BackOffice;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;
using RebelCms.Cms.Web.Mvc.Metadata;

namespace RebelCms.Cms.Web.PropertyEditors.Label
{
    /// <summary>
    /// The model for the label property editor
    /// </summary>
    [EmbeddedView("RebelCms.Cms.Web.PropertyEditors.Label.Views.LabelEditor.cshtml", "RebelCms.Cms.Web.PropertyEditors")]
    public class LabelEditorModel : EditorModel
    {
        /// <summary>
        /// The Label value
        /// </summary>
        [ShowLabel(false)]
        public string Value { get; set; }
    }
}
