using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Cms.Web.PropertyEditors.UserGroupPicker
{
    [PropertyEditor(CorePluginConstants.UserGroupPickerPropertyEditorId, "UserGroupPicker", "User Group Picker", IsParameterEditor = true)]
    public class UserGroupPickerEditor : PropertyEditor<UserGroupPickerEditorModel, UserGroupPickerPreValueModel>
    {
        private IUmbracoApplicationContext _applicationContext;

        public UserGroupPickerEditor(IUmbracoApplicationContext applicationContext)
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
