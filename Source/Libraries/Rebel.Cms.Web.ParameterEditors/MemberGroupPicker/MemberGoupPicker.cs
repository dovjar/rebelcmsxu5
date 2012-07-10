using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Cms.Web;
using Rebel.Cms.Web.Model.BackOffice.ParameterEditors;

namespace Rebel.Cms.Web.ParameterEditors.MemberGroupPicker
{
    [ParameterEditor("EA9C7F7F-042B-40C9-B595-262965AAF98B", "MemberGroupPicker", "Member Group Picker", CorePluginConstants.UserGroupPickerPropertyEditorId)]
    public class MemberGroupPicker : ParameterEditor
    {
        public MemberGroupPicker(IPropertyEditorFactory propertyEditorFactory) 
            : base(propertyEditorFactory)
        { }

        public override string PropertyEditorPreValues
        {
            get
            {
                return @"
                    <preValues>
                        <preValue name='Type'><![CDATA[Member]]></preValue>
                    </preValues>";
            }
        }
    }
}
