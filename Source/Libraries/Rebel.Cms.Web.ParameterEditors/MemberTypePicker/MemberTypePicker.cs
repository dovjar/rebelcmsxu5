using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Cms.Web;
using Rebel.Cms.Web.Model.BackOffice.ParameterEditors;

namespace Rebel.Cms.Web.ParameterEditors.MemberTypePicker
{
    [ParameterEditor("2AA7E19B-7BE1-42A5-935A-3CAA25A78D24", "MemberTypePicker", "Member Type Picker", CorePluginConstants.TypePickerPropertyEditorId)]
    public class MemberTypePicker : ParameterEditor
    {
        public MemberTypePicker(IPropertyEditorFactory propertyEditorFactory) 
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
