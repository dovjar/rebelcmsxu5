using System.ComponentModel;
using System.Collections.Generic;
using Rebel.Cms.Web.EmbeddedViewEngine;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System;
using System.Linq;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Rebel.Cms.Web.PropertyEditors.UrlPicker
{
    public class UrlPickerEditorModel : EditorModel<UrlPickerPreValueModel>
    {
        public UrlPickerEditorModel(UrlPickerPreValueModel preValueModel)
            : base(preValueModel)
        {

        }

        /// <summary>
        /// Whether the user can do their thang
        /// </summary>
        [DisplayName("Do your thang")]
        public bool ThangDoer { get; set; }
    }
}
