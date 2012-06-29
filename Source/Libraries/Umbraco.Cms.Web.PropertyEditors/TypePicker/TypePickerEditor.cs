using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Cms.Web.PropertyEditors.TypePicker
{
    [PropertyEditor(CorePluginConstants.TypePickerPropertyEditorId, "TypePicker", "Type Picker", IsParameterEditor = true)]
    public class TypePickerEditor : PropertyEditor<TypePickerEditorModel, TypePickerPreValueModel>
    {
        private IUmbracoApplicationContext _applicationContext;

        public TypePickerEditor(IUmbracoApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public override TypePickerEditorModel CreateEditorModel(TypePickerPreValueModel preValues)
        {
            return new TypePickerEditorModel(_applicationContext, preValues);
        }

        public override TypePickerPreValueModel CreatePreValueEditorModel()
        {
            return new TypePickerPreValueModel();
        }
    }
}
