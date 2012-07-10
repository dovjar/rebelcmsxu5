using System.Collections.Generic;
using Rebel.Cms.Web.EmbeddedViewEngine;
using System.Web.Script.Serialization;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Rebel.Cms.Web.PropertyEditors.Slider
{
	[EmbeddedView("Rebel.Cms.Web.PropertyEditors.Slider.Views.SliderEditor.cshtml", "Rebel.Cms.Web.PropertyEditors")]
	public class SliderEditorModel : EditorModel<SliderPreValueModel>
	{
		public SliderEditorModel(SliderPreValueModel preValueModel)
			: base(preValueModel)
		{
			this.Defaults = preValueModel;
		}

		public SliderPreValueModel Defaults { get; set; }

		public string Value { get; set; }

		public string SliderControl { get; set; }

		public override IDictionary<string, object> GetSerializedValue()
		{
			return new Dictionary<string, object> { { "Value", this.Value } };
		}

		public override void SetModelValues(IDictionary<string, object> serializedVal)
		{
			if (serializedVal == null || !serializedVal.ContainsKey("Value"))
			{
				return;
			}

			// set the data/value
			this.Value = serializedVal["Value"].ToString();

			// split the data/value
			int value1, value2;
			var values = this.Value.Split(',');

			// parse out the first value
			if (int.TryParse(values[0], out value1))
			{
				this.Defaults.Value = value1;

				// parse out the second value
				if (values.Length > 1 && int.TryParse(values[1], out value2))
				{
					this.Defaults.Value2 = value2;
				}
			}

			base.SetModelValues(serializedVal);
		}
	}
}