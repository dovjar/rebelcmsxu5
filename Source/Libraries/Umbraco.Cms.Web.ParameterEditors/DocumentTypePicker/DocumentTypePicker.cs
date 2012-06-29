using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Model.BackOffice.ParameterEditors;

namespace Umbraco.Cms.Web.ParameterEditors.DocumentTypePicker
{
    [ParameterEditor("DCD81193-9C21-470B-9857-10098EA31226", "DocumentTypePicker", "Document Type Picker", CorePluginConstants.TypePickerPropertyEditorId)]
    public class DocumentTypePicker : ParameterEditor
    {
        public DocumentTypePicker(IPropertyEditorFactory propertyEditorFactory) 
            : base(propertyEditorFactory)
        { }

        public override string PropertyEditorPreValues
        {
            get
            {
                return @"
                    <preValues>
                        <preValue name='Type'><![CDATA[Document]]></preValue>
                    </preValues>";
            }
        }
    }
}
