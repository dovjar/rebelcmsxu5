using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Cms.Web;
using Rebel.Cms.Web.Model.BackOffice.ParameterEditors;

namespace Rebel.Cms.Web.ParameterEditors.UserGroupPicker
{
    [ParameterEditor("1810554C-2852-4038-83BF-C347D4ADCA57", "UserGroupPicker", "User Group Picker", CorePluginConstants.UserGroupPickerPropertyEditorId)]
    public class UserGroupPicker : ParameterEditor
    {
        public UserGroupPicker(IPropertyEditorFactory propertyEditorFactory) 
            : base(propertyEditorFactory)
        { }

        public override string PropertyEditorPreValues
        {
            get
            {
                return @"
                    <preValues>
                        <preValue name='Type'><![CDATA[User]]></preValue>
                    </preValues>";
            }
        }
    }
}
