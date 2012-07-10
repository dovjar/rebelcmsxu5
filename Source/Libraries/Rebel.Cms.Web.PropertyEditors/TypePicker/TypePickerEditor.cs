using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Rebel.Cms.Web.PropertyEditors.TypePicker
{
    [PropertyEditor(CorePluginConstants.TypePickerPropertyEditorId, "TypePicker", "Type Picker", IsParameterEditor = true)]
    public class TypePickerEditor : PropertyEditor<TypePickerEditorModel, TypePickerPreValueModel>
    {
        private IRebelApplicationContext _applicationContext;

        public TypePickerEditor(IRebelApplicationContext applicationContext)
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
