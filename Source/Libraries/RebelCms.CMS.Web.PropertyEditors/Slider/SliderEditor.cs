using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;

namespace RebelCms.Cms.Web.PropertyEditors.Slider
{
    [PropertyEditor(CorePluginConstants.SliderPropertyEditorId, "Slider", "Slider")]
	public class SliderEditor : PropertyEditor<SliderEditorModel, SliderPreValueModel>
	{
		public override SliderEditorModel CreateEditorModel(SliderPreValueModel preValues)
		{
			return new SliderEditorModel(preValues);
		}

		public override SliderPreValueModel CreatePreValueEditorModel()
		{
			return new SliderPreValueModel();
		}
	}
}