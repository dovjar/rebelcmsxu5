using System.Collections.Generic;
using RebelCms.Cms.Web.EmbeddedViewEngine;
using System.Web.Script.Serialization;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;

namespace RebelCms.Cms.Web.PropertyEditors.Slider
{
	[EmbeddedView("RebelCms.Cms.Web.PropertyEditors.Slider.Views.SliderEditor.cshtml", "RebelCms.Cms.Web.PropertyEditors")]
	public class SliderEditorModel : EditorModel<SliderPreValueModel>
	{
		public SliderEditorModel(SliderPreValueModel preValueModel)
			: base(preValueModel)
		{
		}		

		public string Value { get; set; }

		public string SliderControl { get; set; }

       
	}
}