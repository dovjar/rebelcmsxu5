using System;
using System.Web.UI;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;
using RebelCms.Framework;

[assembly: WebResource("RebelCms.Cms.Web.PropertyEditors.RichTextBox.Resources.RTEPreValueEditor.js", "application/x-javascript")]
[assembly: WebResource("RebelCms.Cms.Web.PropertyEditors.RichTextBox.Resources.RichTextBox.js", "application/x-javascript")]
[assembly: WebResource("RebelCms.Cms.Web.PropertyEditors.RichTextBox.Resources.SizeInputField.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("RebelCms.Cms.Web.PropertyEditors.RichTextBox.Resources.FeaturesCheckboxList.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("RebelCms.Cms.Web.PropertyEditors.RichTextBox.Resources.StylesheetsCheckboxList.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("RebelCms.Cms.Web.PropertyEditors.RichTextBox.Resources.FeaturesIcons.gif", "image/gif")]

namespace RebelCms.Cms.Web.PropertyEditors.RichTextBox
{
    [PropertyEditor(CorePluginConstants.RichTextBoxPropertyEditorId, "RichTextBox", "WYSIWYG Editor (RTE)")]
    public class RichTextBoxEditor : ContentAwarePropertyEditor<RichTextBoxEditorModel, RichTextBoxPreValueModel>
    {
        private IRebelCmsApplicationContext _appContext;

        public RichTextBoxEditor(IRebelCmsApplicationContext appContext)
        {
            _appContext = appContext;
        }

        public override RichTextBoxEditorModel CreateEditorModel(RichTextBoxPreValueModel preValues)
        {
            return new RichTextBoxEditorModel(preValues, _appContext, GetContentModelValue(x => x.Id, HiveId.Empty));
        }

        public override RichTextBoxPreValueModel CreatePreValueEditorModel()
        {
            return new RichTextBoxPreValueModel(_appContext);
        }

    }
}
