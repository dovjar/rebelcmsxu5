using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Rebel.Cms.Web.PropertyEditors.UserGroupPicker
{
    [PropertyEditor(CorePluginConstants.UserGroupPickerPropertyEditorId, "UserGroupPicker", "User Group Picker", IsParameterEditor = true)]
    public class UserGroupPickerEditor : PropertyEditor<UserGroupPickerEditorModel, UserGroupPickerPreValueModel>
    {
        private IRebelApplicationContext _applicationContext;

        public UserGroupPickerEditor(IRebelApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public override UserGroupPickerEditorModel CreateEditorModel(UserGroupPickerPreValueModel preValues)
        {
            return new UserGroupPickerEditorModel(_applicationContext, preValues);
        }

        public override UserGroupPickerPreValueModel CreatePreValueEditorModel()
        {
            return new UserGroupPickerPreValueModel();
        }
    }
}
